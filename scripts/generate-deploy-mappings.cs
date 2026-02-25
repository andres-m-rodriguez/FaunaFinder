#!/usr/bin/env dotnet run

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

var repoRoot = FindRepoRoot(Environment.CurrentDirectory);
var appHostPath = Path.Combine(repoRoot, "src", "FaunaFinder.AppHost");
var programCs = Path.Combine(appHostPath, "Program.cs");
var outputPath = Path.Combine(repoRoot, "deployable-resources.json");

Console.WriteLine($"Analyzing {programCs}...");

var programContent = File.ReadAllText(programCs);
var matches = AddProjectRegex().Matches(programContent);

var resources = new Dictionary<string, ResourceInfo>();

foreach (Match match in matches)
{
    var projectTypeName = match.Groups[1].Value;
    var resourceName = match.Groups[2].Value;
    var projectName = projectTypeName.Replace("_", ".");

    Console.WriteLine($"Found resource: {resourceName} -> {projectName}");

    var projectFile = FindProjectFile(repoRoot, projectName);
    if (projectFile is null)
    {
        Console.WriteLine($"  Warning: Could not find project file for {projectName}");
        continue;
    }

    var dependencies = GetAllDependencies(projectFile);
    resources[resourceName] = new ResourceInfo(projectName, [.. dependencies.Order()]);
    Console.WriteLine($"  Dependencies: {dependencies.Count}");
}

// Build reverse mapping: project -> resources it affects
var projectToResources = new Dictionary<string, List<string>>();

foreach (var (resourceName, info) in resources)
{
    AddMapping(projectToResources, info.Project, resourceName);
    foreach (var dep in info.Dependencies)
        AddMapping(projectToResources, dep, resourceName);
}

var output = new DeployMappings(
    resources,
    projectToResources.ToDictionary(p => p.Key, p => p.Value.Distinct().Order().ToList())
);

var json = JsonSerializer.Serialize(output, AppJsonContext.Default.DeployMappings);
File.WriteAllText(outputPath, json);
Console.WriteLine($"\nGenerated {outputPath}");

return;

// --- Helpers ---

static string FindRepoRoot(string startPath)
{
    for (var dir = new DirectoryInfo(startPath); dir is not null; dir = dir.Parent)
        if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            return dir.FullName;
    return startPath;
}

static string? FindProjectFile(string repoRoot, string projectName) =>
    Directory.GetFiles(repoRoot, $"{projectName}.csproj", SearchOption.AllDirectories).FirstOrDefault();

static HashSet<string> GetAllDependencies(string projectFile)
{
    var dependencies = new HashSet<string>();
    var visited = new HashSet<string>();
    CollectDependencies(projectFile, dependencies, visited);
    return dependencies;
}

static void CollectDependencies(string projectFile, HashSet<string> dependencies, HashSet<string> visited)
{
    if (!File.Exists(projectFile) || !visited.Add(projectFile))
        return;

    try
    {
        var doc = XDocument.Load(projectFile);
        var projectRefs = doc.Descendants("ProjectReference")
            .Select(e => e.Attribute("Include")?.Value)
            .OfType<string>();

        foreach (var refPath in projectRefs)
        {
            var fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectFile)!, refPath));
            var projectName = Path.GetFileNameWithoutExtension(fullPath);

            if (projectName.Contains("Generators") || projectName.Contains("Aspire"))
                continue;

            dependencies.Add(projectName);
            CollectDependencies(fullPath, dependencies, visited);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Warning: Error parsing {projectFile}: {ex.Message}");
    }
}

static void AddMapping(Dictionary<string, List<string>> dict, string key, string value)
{
    if (!dict.TryGetValue(key, out var list))
        dict[key] = list = [];
    if (!list.Contains(value))
        list.Add(value);
}

partial class Program
{
    [GeneratedRegex(@"\.AddProject<Projects\.(\w+)>\s*\(\s*""(\w+)""")]
    private static partial Regex AddProjectRegex();
}

// --- Types ---

record ResourceInfo(
    [property: JsonPropertyName("project")] string Project,
    [property: JsonPropertyName("dependencies")] List<string> Dependencies
);

record DeployMappings(
    [property: JsonPropertyName("resources")] Dictionary<string, ResourceInfo> Resources,
    [property: JsonPropertyName("projectToResources")] Dictionary<string, List<string>> ProjectToResources
);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DeployMappings))]
partial class AppJsonContext : JsonSerializerContext;
