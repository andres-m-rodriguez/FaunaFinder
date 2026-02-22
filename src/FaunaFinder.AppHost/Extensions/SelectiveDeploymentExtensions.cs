using Microsoft.Extensions.Logging;

namespace FaunaFinder.AppHost.Extensions;

/// <summary>
/// Extensions for selective deployment based on affected projects from dotnet-affected.
/// dotnet-affected already handles transitive dependencies - if a shared project changes,
/// all projects that depend on it will be in the affected list.
/// </summary>
public static class SelectiveDeploymentExtensions
{
    private const string AffectedProjectsEnvVar = "AFFECTED_PROJECTS";
    private const string ForceDeployAllEnvVar = "FORCE_DEPLOY_ALL";

    /// <summary>
    /// Mapping of Aspire resource names to their corresponding project names.
    /// </summary>
    public static readonly Dictionary<string, string> ResourceToProject = new()
    {
        ["server"] = "FaunaFinder.Server",
        ["seeder"] = "FaunaFinder.Seeder",
    };

    /// <summary>
    /// Gets the list of affected projects from the environment variable.
    /// </summary>
    public static string[] GetAffectedProjects()
    {
        var affected = Environment.GetEnvironmentVariable(AffectedProjectsEnvVar);
        if (string.IsNullOrWhiteSpace(affected))
            return [];

        return affected.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// Checks if force deploy all is enabled.
    /// </summary>
    public static bool IsForceDeployAll() =>
        string.Equals(
            Environment.GetEnvironmentVariable(ForceDeployAllEnvVar),
            "true",
            StringComparison.OrdinalIgnoreCase
        );

    /// <summary>
    /// Checks if a resource should be deployed based on affected projects.
    /// dotnet-affected handles transitive dependencies, so we just check if the
    /// deployable project is in the affected list.
    /// </summary>
    public static bool ShouldDeployResource(string resourceName)
    {
        if (IsForceDeployAll())
            return true;

        var affectedProjects = GetAffectedProjects();

        // No affected projects info means deploy everything (local dev or full deploy)
        if (affectedProjects.Length == 0)
            return true;

        // Get the project name for this resource
        if (!ResourceToProject.TryGetValue(resourceName, out var projectName))
            return true; // Unknown resource - deploy by default

        // Check if the project is in the affected list
        return affectedProjects.Any(p =>
            p.Contains(projectName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Logs the selective deployment status.
    /// </summary>
    public static void LogDeploymentStatus(ILogger logger)
    {
        var affectedProjects = GetAffectedProjects();

        if (IsForceDeployAll())
        {
            logger.LogInformation("Force deploy all enabled");
            return;
        }

        if (affectedProjects.Length == 0)
        {
            logger.LogInformation("No AFFECTED_PROJECTS set - deploying all resources");
            return;
        }

        logger.LogInformation("Affected projects: {Projects}", string.Join(", ", affectedProjects));

        foreach (var (resource, project) in ResourceToProject)
        {
            var shouldDeploy = ShouldDeployResource(resource);
            logger.LogInformation("  {Resource}: {Status}", resource, shouldDeploy ? "DEPLOY" : "SKIP");
        }
    }
}
