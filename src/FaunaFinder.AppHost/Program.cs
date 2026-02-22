using FaunaFinder.AppHost.Extensions;
using Microsoft.Extensions.Logging;

#pragma warning disable ASPIREPIPELINES001

var builder = DistributedApplication.CreateBuilder(args);

// Azure Container App Environment (required for PublishAsAzureContainerAppJob)
builder.AddAzureContainerAppEnvironment("aca-env");

var postgres = builder
    .AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(c =>
        c.WithImage("postgis/postgis", "17-3.5")
            .WithDataVolume("faunafinder-postgres-data")
            .WithPgAdmin()
    );

var identityDb = postgres.AddDatabase("faunafinder-identity").WithDropDatabaseCommand();
var wildlifeDb = postgres.AddDatabase("faunafinder-wildlife").WithDropDatabaseCommand();
var mainDb = postgres.AddDatabase("faunafinder").WithDropDatabaseCommand();

// Database seeder - runs as a job in Azure, triggered by pipeline step
var seeder = builder
    .AddProject<Projects.FaunaFinder_Seeder>("seeder")
    .WithReference(mainDb)
    .WithReference(identityDb)
    .WithReference(wildlifeDb)
    .WaitFor(mainDb)
    .WaitFor(identityDb)
    .WaitFor(wildlifeDb)
    .PublishAsAzureContainerAppJob();

// Pipeline step to run migrations after deployment
builder.Pipeline.AddStep(
    "run-migrations",
    async (context) =>
    {
        var task = await context.ReportingStep.CreateTaskAsync(
            "Running database migrations",
            context.CancellationToken
        );

        await using (task.ConfigureAwait(false))
        {
            var resourceGroup = Environment.GetEnvironmentVariable("AZURE_RESOURCE_GROUP");
            if (string.IsNullOrEmpty(resourceGroup))
            {
                context.Logger.LogWarning(
                    "AZURE_RESOURCE_GROUP not set, skipping migration trigger"
                );
                return;
            }

            context.Logger.LogInformation("Triggering seeder job to apply migrations...");

            // Start the job and wait for completion (az containerapp job start waits by default)
            var (exitCode, output, error) = await RunAzCommandAsync(
                $"containerapp job start --name seeder --resource-group {resourceGroup}",
                context.CancellationToken
            );

            if (exitCode != 0)
            {
                context.Logger.LogError("Failed to start seeder job: {Error}", error);
                throw new InvalidOperationException($"Migration job failed to start: {error}");
            }

            context.Logger.LogInformation("Seeder job completed successfully");
        }
    },
    dependsOn: "deploy"
);

static async Task<(int exitCode, string output, string error)> RunAzCommandAsync(
    string arguments,
    CancellationToken ct
)
{
    var process = new System.Diagnostics.Process
    {
        StartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "az",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        },
    };

    process.Start();
    var output = await process.StandardOutput.ReadToEndAsync(ct);
    var error = await process.StandardError.ReadToEndAsync(ct);
    await process.WaitForExitAsync(ct);

    return (process.ExitCode, output, error);
}

// API + WASM Client
builder
    .AddProject<Projects.FaunaFinder_Server>("server")
    .WithReference(mainDb)
    .WithReference(identityDb)
    .WithReference(wildlifeDb)
    .WaitFor(mainDb)
    .WaitFor(identityDb)
    .WaitFor(wildlifeDb)
    .WithExternalHttpEndpoints();

builder.Build().Run();
