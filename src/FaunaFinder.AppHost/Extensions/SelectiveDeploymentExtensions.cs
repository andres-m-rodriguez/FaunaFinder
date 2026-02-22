using Microsoft.Extensions.Logging;

namespace FaunaFinder.AppHost.Extensions;

/// <summary>
/// Extensions for selective deployment based on affected projects from dotnet-affected.
/// </summary>
public static class SelectiveDeploymentExtensions
{
    private const string AffectedProjectsEnvVar = "AFFECTED_PROJECTS";
    private const string ForceDeployAllEnvVar = "FORCE_DEPLOY_ALL";

    /// <summary>
    /// Mapping of resource names to project patterns.
    /// When a project matching any pattern changes, the corresponding resource should be deployed.
    /// </summary>
    public static readonly Dictionary<string, string[]> ResourceToProjectPatterns = new()
    {
        ["server"] =
        [
            "FaunaFinder.Server",
            "FaunaFinder.Client",
            "FaunaFinder.ServiceDefaults",
            ".Api",
            ".Application",
            ".Contracts",
            ".DataAccess",
            "i18n",
            "Pagination"
        ],
        ["seeder"] =
        [
            "FaunaFinder.Seeder",
            ".Database"
        ]
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
    /// </summary>
    public static bool ShouldDeployResource(string resourceName)
    {
        if (IsForceDeployAll())
            return true;

        var affectedProjects = GetAffectedProjects();

        // No affected projects info means deploy everything (local dev or full deploy)
        if (affectedProjects.Length == 0)
            return true;

        if (!ResourceToProjectPatterns.TryGetValue(resourceName, out var patterns))
            return true; // Unknown resource - deploy by default

        return affectedProjects.Any(project =>
            patterns.Any(pattern => project.Contains(pattern, StringComparison.OrdinalIgnoreCase))
        );
    }

    /// <summary>
    /// Gets all resources that should be deployed based on affected projects.
    /// </summary>
    public static IEnumerable<string> GetAffectedResources()
    {
        if (IsForceDeployAll())
            return ResourceToProjectPatterns.Keys;

        var affectedProjects = GetAffectedProjects();

        if (affectedProjects.Length == 0)
            return ResourceToProjectPatterns.Keys;

        return ResourceToProjectPatterns
            .Where(kvp =>
                affectedProjects.Any(project =>
                    kvp.Value.Any(pattern =>
                        project.Contains(pattern, StringComparison.OrdinalIgnoreCase)
                    )
                )
            )
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Logs the selective deployment status.
    /// </summary>
    public static void LogDeploymentStatus(ILogger logger)
    {
        var affectedProjects = GetAffectedProjects();
        var forceDeployAll = IsForceDeployAll();

        if (forceDeployAll)
        {
            logger.LogInformation("Force deploy all enabled - deploying all resources");
            return;
        }

        if (affectedProjects.Length == 0)
        {
            logger.LogInformation("No AFFECTED_PROJECTS set - deploying all resources");
            return;
        }

        logger.LogInformation("Affected projects: {Projects}", string.Join(", ", affectedProjects));

        var affectedResources = GetAffectedResources().ToList();
        logger.LogInformation(
            "Resources to deploy: {Resources}",
            string.Join(", ", affectedResources)
        );
    }
}
