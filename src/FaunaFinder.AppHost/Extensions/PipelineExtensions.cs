#pragma warning disable ASPIREPIPELINES001

using Microsoft.Extensions.Logging;
using static FaunaFinder.AppHost.Extensions.AzureCliHelper;

namespace FaunaFinder.AppHost.Extensions;

public static class PipelineExtensions
{
    public static IDistributedApplicationBuilder AddMigrationsStep(
        this IDistributedApplicationBuilder builder
    )
    {
        builder.Pipeline.AddStep(
            "run-migrations",
            async (context) =>
            {
                if (string.IsNullOrEmpty(ResourceGroup))
                {
                    context.Logger.LogWarning("AZURE_RESOURCE_GROUP not set, skipping migrations");
                    return;
                }

                var task = await context.ReportingStep.CreateTaskAsync(
                    "Running database migrations",
                    context.CancellationToken
                );

                await using (task.ConfigureAwait(false))
                {
                    context.Logger.LogInformation("Triggering seeder job to apply migrations...");

                    var (exitCode, _, error) = await RunCommandAsync(
                        $"containerapp job start --name seeder --resource-group {ResourceGroup}",
                        context.CancellationToken
                    );

                    if (exitCode != 0)
                    {
                        context.Logger.LogError("Failed to start seeder job: {Error}", error);
                        throw new InvalidOperationException($"Migration job failed: {error}");
                    }

                    context.Logger.LogInformation("Seeder job completed successfully");
                }
            },
            dependsOn: "deploy-seeder"
        );

        return builder;
    }

    public static IDistributedApplicationBuilder AddCustomDomainStep(
        this IDistributedApplicationBuilder builder
    )
    {
        builder.Pipeline.AddStep(
            "configure-custom-domain",
            async (context) =>
            {
                if (string.IsNullOrEmpty(CustomDomain))
                {
                    context.Logger.LogInformation("CUSTOM_DOMAIN not set, skipping");
                    return;
                }

                if (string.IsNullOrEmpty(ResourceGroup))
                {
                    context.Logger.LogWarning(
                        "AZURE_RESOURCE_GROUP not set, skipping custom domain"
                    );
                    return;
                }

                var task = await context.ReportingStep.CreateTaskAsync(
                    $"Configuring custom domain: {CustomDomain}",
                    context.CancellationToken
                );

                await using (task.ConfigureAwait(false))
                {
                    // Get the environment name from the server container app
                    var (envExitCode, envOutput, envError) = await RunCommandAsync(
                        $"containerapp show --name server --resource-group {ResourceGroup} --query properties.environmentId --output tsv",
                        context.CancellationToken
                    );

                    if (envExitCode != 0)
                    {
                        context.Logger.LogError("Failed to get environment ID: {Error}", envError);
                        throw new InvalidOperationException(
                            $"Failed to get environment ID: {envError}"
                        );
                    }

                    var environmentName = envOutput.Trim().Split('/').Last();

                    // Add hostname (may already exist, that's OK)
                    context.Logger.LogInformation(
                        "Adding custom hostname {Domain}...",
                        CustomDomain
                    );
                    var (addExitCode, _, addError) = await RunCommandAsync(
                        $"containerapp hostname add --name server --resource-group {ResourceGroup} --hostname {CustomDomain}",
                        context.CancellationToken
                    );

                    if (
                        addExitCode != 0
                        && !addError.Contains("already exists", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        context.Logger.LogWarning("Hostname add returned: {Error}", addError);
                    }

                    // Bind with managed certificate
                    context.Logger.LogInformation("Binding hostname with managed certificate...");
                    var (bindExitCode, _, bindError) = await RunCommandAsync(
                        $"containerapp hostname bind --name server --resource-group {ResourceGroup} --hostname {CustomDomain} --environment {environmentName} --validation-method HTTP",
                        context.CancellationToken
                    );

                    if (
                        bindExitCode != 0
                        && !bindError.Contains("already", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        context.Logger.LogError("Failed to bind hostname: {Error}", bindError);
                        throw new InvalidOperationException(
                            $"Failed to bind custom domain: {bindError}"
                        );
                    }

                    context.Logger.LogInformation(
                        "Custom domain {Domain} configured successfully",
                        CustomDomain
                    );
                }
            },
            dependsOn: "deploy-server"
        );

        return builder;
    }
}
