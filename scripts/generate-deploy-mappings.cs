#!/usr/bin/env dotnet run

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

var repoRoot = FindRepoRoot(Environment.CurrentDirectory);
var programCs = Path.Combine(repoRoot, "src", "FaunaFinder.AppHost", "Program.cs");
var outputPath = Path.Combine(repoRoot, "deployable-resources.json");

Console.WriteLine($"Analyzing {programCs}...");

var programContent = File.ReadAllText(programCs);
var matches = AddProjectRegex().Matches(programContent);

var resources = new Dictionary<string, ResourceInfo>();
var projectToResources = new Dictionary<string, List<string>>();

foreach (Match match in matches)
{
    var projectName = match.Groups[1].Value.Replace("_", ".");
    var resourceName = match.Groups[2].Value;

    Console.WriteLine($"Found resource: {resourceName} -> {projectName}");

    var projectFile = Directory
        .GetFiles(repoRoot, $"{projectName}.csproj", SearchOption.AllDirectories)
        .FirstOrDefault();

    if (projectFile is null)
    {
        Console.WriteLine($"  Warning: Could not find project file for {projectName}");
        continue;
    }

    var dependencies = GetAllDependencies(projectFile);
    Console.WriteLine($"  Dependencies: {dependencies.Count}");

    resources[resourceName] = new ResourceInfo(projectName, [.. dependencies.Order()]);

    AddMapping(projectToResources, projectName, resourceName);
    foreach (var dep in dependencies)
        AddMapping(projectToResources, dep, resourceName);
}

var output = new DeployMappings(
    resources,
    projectToResources
        .OrderBy(p => p.Key)
        .ToDictionary(p => p.Key, p => p.Value.Distinct().Order().ToList())
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

static HashSet<string> GetAllDependencies(string projectFile)
{
    var dependencies = new HashSet<string>();
    var visited = new HashSet<string>();

    void Collect(string file)
    {
        if (!File.Exists(file) || !visited.Add(file))
            return;

        try
        {
            foreach (var refPath in XDocument.Load(file)
                .Descendants("ProjectReference")
                .Select(e => e.Attribute("Include")?.Value)
                .OfType<string>())
            {
                var fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, refPath));
                var name = Path.GetFileNameWithoutExtension(fullPath);

                if (name.Contains("Generators") || name.Contains("Aspire"))
                    continue;

                dependencies.Add(name);
                Collect(fullPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: {ex.Message}");
        }
    }

    Collect(projectFile);
    return dependencies;
}

static void AddMapping(Dictionary<string, List<string>> dict, string key, string value)
{
    if (!dict.TryGetValue(key, out var list))
        dict[key] = list = [];
    if (!list.Contains(value))
        list.Add(value);
}

// --- Types ---

partial class Program
{
    [GeneratedRegex(@"\.AddProject<Projects\.(\w+)>\s*\(\s*""(\w+)""")]
    private static partial Regex AddProjectRegex();
}

record ResourceInfo(string Project, List<string> Dependencies);

record DeployMappings(
    Dictionary<string, ResourceInfo> Resources,
    Dictionary<string, List<string>> ProjectToResources
);

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DeployMappings))]
partial class AppJsonContext : JsonSerializerContext;
