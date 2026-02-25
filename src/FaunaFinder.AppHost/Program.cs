using FaunaFinder.AppHost.Extensions;

#pragma warning disable ASPIREPIPELINES001

var builder = DistributedApplication.CreateBuilder(args);

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

builder
    .AddProject<Projects.FaunaFinder_Seeder>("seeder")
    .WithReference(mainDb)
    .WithReference(identityDb)
    .WithReference(wildlifeDb)
    .WaitFor(mainDb)
    .WaitFor(identityDb)
    .WaitFor(wildlifeDb)
    .PublishAsAzureContainerAppJob();

builder
    .AddProject<Projects.FaunaFinder_Server>("server")
    .WithReference(mainDb)
    .WithReference(identityDb)
    .WithReference(wildlifeDb)
    .WaitFor(mainDb)
    .WaitFor(identityDb)
    .WaitFor(wildlifeDb)
    .WithExternalHttpEndpoints();

// Pipeline steps for Azure deployment
builder
    .AddMigrationsStep()
    .AddCustomDomainStep();

builder.Build().Run();
