using FaunaFinder.AppHost.Extensions;
using Microsoft.Extensions.Logging;

#pragma warning disable ASPIREPIPELINES001

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(c => c
        .WithImage("postgis/postgis", "17-3.5")
        .WithDataVolume("faunafinder-postgres-data")
        .WithPgAdmin());

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

// Pipeline step to run migrations after provisioning
builder.Pipeline.AddStep("run-migrations", async (context) =>
{
    var task = await context.ReportingStep
        .CreateTaskAsync("Running database migrations", context.CancellationToken);

    await using (task.ConfigureAwait(false))
    {
        context.Logger.LogInformation("Triggering seeder job to apply migrations...");

        // Get resource group and job name from provisioned resources
        var resourceGroup = Environment.GetEnvironmentVariable("AZURE_RESOURCE_GROUP");
        if (string.IsNullOrEmpty(resourceGroup))
        {
            context.Logger.LogWarning("AZURE_RESOURCE_GROUP not set, skipping migration trigger");
            return;
        }

        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "az",
                Arguments = $"containerapp job start --name seeder --resource-group {resourceGroup}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(context.CancellationToken);

        if (process.ExitCode != 0)
        {
            context.Logger.LogError("Failed to trigger seeder job: {Error}", error);
            throw new InvalidOperationException($"Migration job failed: {error}");
        }

        context.Logger.LogInformation("Seeder job triggered successfully: {Output}", output);
    }
}, dependsOn: "provision", requiredBy: "deploy");

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
