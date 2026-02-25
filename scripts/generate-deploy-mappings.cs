#!/usr/bin/env dotnet run

using System.Text.RegularExpressions;
using System.Xml.Linq;

var repoRoot = FindRepoRoot(Environment.CurrentDirectory);
var appHostPath = Path.Combine(repoRoot, "src", "FaunaFinder.AppHost");
var programCs = Path.Combine(appHostPath, "Program.cs");
var outputPath = Path.Combine(repoRoot, "deployable-resources.json");

Console.WriteLine($"Analyzing {programCs}...");

// Parse Program.cs to find AddProject<T>("name") calls
var programContent = File.ReadAllText(programCs);
var addProjectPattern = new Regex(@"\.AddProject<Projects\.(\w+)>\s*\(\s*""(\w+)""", RegexOptions.Compiled);
var matches = addProjectPattern.Matches(programContent);

var resources = new Dictionary<string, ResourceInfo>();

foreach (Match match in matches)
{
    var projectTypeName = match.Groups[1].Value; // e.g., FaunaFinder_Server
    var resourceName = match.Groups[2].Value;    // e.g., server

    // Convert FaunaFinder_Server to FaunaFinder.Server
    var projectName = projectTypeName.Replace("_", ".");

    Console.WriteLine($"Found resource: {resourceName} -> {projectName}");

    // Find the project file
    var projectFile = FindProjectFile(repoRoot, projectName);
    if (projectFile == null)
    {
        Console.WriteLine($"  Warning: Could not find project file for {projectName}");
        continue;
    }

    // Get all dependencies (recursive)
    var dependencies = GetAllDependencies(projectFile, repoRoot);

    resources[resourceName] = new ResourceInfo
    {
        Project = projectName,
        Dependencies = dependencies.Order().ToList()
    };

    Console.WriteLine($"  Dependencies: {dependencies.Count}");
}

// Build reverse mapping: project -> resources it affects
var projectToResources = new Dictionary<string, List<string>>();

foreach (var (resourceName, info) in resources)
{
    // The main project affects this resource
    AddMapping(projectToResources, info.Project, resourceName);

    // All dependencies also affect this resource
    foreach (var dep in info.Dependencies)
    {
        AddMapping(projectToResources, dep, resourceName);
    }
}

// Build JSON manually to avoid AOT issues
var json = BuildJson(resources, projectToResources);

static string BuildJson(Dictionary<string, ResourceInfo> resources, Dictionary<string, List<string>> projectToResources)
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("{");

    // Resources section
    sb.AppendLine("  \"resources\": {");
    var resourceEntries = resources.ToList();
    for (int i = 0; i < resourceEntries.Count; i++)
    {
        var (name, info) = resourceEntries[i];
        sb.AppendLine($"    \"{name}\": {{");
        sb.AppendLine($"      \"project\": \"{info.Project}\",");
        sb.Append("      \"dependencies\": [");
        sb.Append(string.Join(", ", info.Dependencies.Select(d => $"\"{d}\"")));
        sb.AppendLine("]");
        sb.Append("    }");
        sb.AppendLine(i < resourceEntries.Count - 1 ? "," : "");
    }
    sb.AppendLine("  },");

    // ProjectToResources section
    sb.AppendLine("  \"projectToResources\": {");
    var projectEntries = projectToResources.OrderBy(p => p.Key).ToList();
    for (int i = 0; i < projectEntries.Count; i++)
    {
        var (project, resList) = projectEntries[i];
        sb.Append($"    \"{project}\": [");
        sb.Append(string.Join(", ", resList.Distinct().Order().Select(r => $"\"{r}\"")));
        sb.Append("]");
        sb.AppendLine(i < projectEntries.Count - 1 ? "," : "");
    }
    sb.AppendLine("  }");

    sb.AppendLine("}");
    return sb.ToString();
}

File.WriteAllText(outputPath, json);
Console.WriteLine($"\nGenerated {outputPath}");

// --- Helper methods ---

static string FindRepoRoot(string startPath)
{
    var dir = new DirectoryInfo(startPath);
    while (dir != null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            return dir.FullName;
        dir = dir.Parent;
    }
    return startPath;
}

static string? FindProjectFile(string repoRoot, string projectName)
{
    var searchPattern = $"{projectName}.csproj";
    var files = Directory.GetFiles(repoRoot, searchPattern, SearchOption.AllDirectories);
    return files.FirstOrDefault();
}

static HashSet<string> GetAllDependencies(string projectFile, string repoRoot)
{
    var dependencies = new HashSet<string>();
    var visited = new HashSet<string>();

    CollectDependencies(projectFile, repoRoot, dependencies, visited);

    return dependencies;
}

static void CollectDependencies(string projectFile, string repoRoot, HashSet<string> dependencies, HashSet<string> visited)
{
    if (!File.Exists(projectFile) || visited.Contains(projectFile))
        return;

    visited.Add(projectFile);

    try
    {
        var doc = XDocument.Load(projectFile);
        var projectRefs = doc.Descendants("ProjectReference")
            .Select(e => e.Attribute("Include")?.Value)
            .Where(v => v != null);

        foreach (var refPath in projectRefs)
        {
            var fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectFile)!, refPath!));
            var projectName = Path.GetFileNameWithoutExtension(fullPath);

            // Skip Aspire generators and other non-deployable projects
            if (projectName.Contains("Generators") || projectName.Contains("Aspire"))
                continue;

            dependencies.Add(projectName);

            // Recursively get dependencies
            CollectDependencies(fullPath, repoRoot, dependencies, visited);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Warning: Error parsing {projectFile}: {ex.Message}");
    }
}

static void AddMapping(Dictionary<string, List<string>> dict, string key, string value)
{
    if (!dict.ContainsKey(key))
        dict[key] = new List<string>();
    if (!dict[key].Contains(value))
        dict[key].Add(value);
}

class ResourceInfo
{
    public string Project { get; set; } = "";
    public List<string> Dependencies { get; set; } = new();
}
