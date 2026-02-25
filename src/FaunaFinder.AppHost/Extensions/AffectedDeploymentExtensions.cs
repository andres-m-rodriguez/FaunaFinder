#pragma warning disable ASPIREPIPELINES001

using FaunaFinder.Aspire.Generators;
using Microsoft.Extensions.Logging;
using static FaunaFinder.AppHost.Extensions.AzureCliHelper;

namespace FaunaFinder.AppHost.Extensions;

public static class AffectedDeploymentExtensions
{
    /// <summary>
    /// Adds a deployment step that only deploys resources whose projects are affected.
    /// The affected projects are read from the AFFECTED_PROJECTS environment variable,
    /// which should be a comma-separated list of project names.
    /// </summary>
    public static IDistributedApplicationBuilder AddAffectedDeploymentStep(
        this IDistributedApplicationBuilder builder
    )
    {
        builder.Pipeline.AddStep(
            "deploy-affected",
            async (context) =>
            {
                var affectedProjectsEnv = GetEnvironmentVariable("AFFECTED_PROJECTS");

                if (string.IsNullOrEmpty(affectedProjectsEnv))
                {
                    context.Logger.LogInformation(
                        "AFFECTED_PROJECTS not set, running dotnet affected..."
                    );

                    // Run dotnet affected to detect changes
                    var affectedProjects = await DetectAffectedProjectsAsync(
                        context.Logger,
                        context.CancellationToken
                    );

                    if (affectedProjects.Count == 0)
                    {
                        context.Logger.LogInformation("No projects affected, skipping deployment");
                        return;
                    }

                    affectedProjectsEnv = string.Join(",", affectedProjects);
                }

                context.Logger.LogInformation("Affected projects: {Projects}", affectedProjectsEnv);

                var affectedProjectsList = affectedProjectsEnv
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Determine which resources to deploy based on affected projects
                var resourcesToDeploy = new List<string>();

                foreach (var (projectName, resourceName) in DeployableResources.ProjectToResource)
                {
                    if (affectedProjectsList.Contains(projectName))
                    {
                        resourcesToDeploy.Add(resourceName);
                        context.Logger.LogInformation(
                            "Project {Project} affected, will deploy {Resource}",
                            projectName,
                            resourceName
                        );
                    }
                }

                if (resourcesToDeploy.Count == 0)
                {
                    context.Logger.LogInformation(
                        "No deployable resources affected, skipping deployment"
                    );
                    return;
                }

                // Set environment variables to signal which resources should be deployed
                foreach (var resource in resourcesToDeploy)
                {
                    var envVarName = $"DEPLOY_{resource.ToUpperInvariant()}";
                    Environment.SetEnvironmentVariable(envVarName, "true");
                    context.Logger.LogInformation("Set {EnvVar}=true", envVarName);
                }
            },
            dependsOn: "restore"
        );

        return builder;
    }

    /// <summary>
    /// Gets the list of affected resources based on the AFFECTED_PROJECTS environment variable.
    /// </summary>
    public static IReadOnlyList<string> GetAffectedResources()
    {
        var affectedProjectsEnv = GetEnvironmentVariable("AFFECTED_PROJECTS");

        if (string.IsNullOrEmpty(affectedProjectsEnv))
        {
            return [];
        }

        var affectedProjectsList = affectedProjectsEnv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var resources = new List<string>();

        foreach (var (projectName, resourceName) in DeployableResources.ProjectToResource)
        {
            if (affectedProjectsList.Contains(projectName))
            {
                resources.Add(resourceName);
            }
        }

        return resources;
    }

    /// <summary>
    /// Checks if a specific resource should be deployed based on affected projects.
    /// </summary>
    public static bool ShouldDeployResource(string resourceName)
    {
        // Check for explicit flag
        var envVarName = $"DEPLOY_{resourceName.ToUpperInvariant()}";
        var deployFlag = GetEnvironmentVariable(envVarName);

        if (!string.IsNullOrEmpty(deployFlag))
        {
            return deployFlag.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        // Check affected projects
        var affectedProjectsEnv = GetEnvironmentVariable("AFFECTED_PROJECTS");

        if (string.IsNullOrEmpty(affectedProjectsEnv))
        {
            // No affected projects specified, deploy everything
            return true;
        }

        var affectedProjectsList = affectedProjectsEnv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Check if this resource's project is affected
        if (DeployableResources.ResourceToProject.TryGetValue(resourceName, out var projectName))
        {
            return affectedProjectsList.Contains(projectName);
        }

        return false;
    }

    private static async Task<List<string>> DetectAffectedProjectsAsync(
        ILogger logger,
        CancellationToken ct
    )
    {
        var result = new List<string>();

        try
        {
            // Run dotnet affected to get the list of affected projects
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "affected -p . --from HEAD~1 --to HEAD --format text",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                },
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var projectName = line.Trim();
                    if (!string.IsNullOrEmpty(projectName))
                    {
                        result.Add(projectName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to run dotnet affected, assuming all projects affected");
        }

        return result;
    }
}
