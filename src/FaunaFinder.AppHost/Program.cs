using FaunaFinder.AppHost.Extensions;

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

// Database seeder (manual job in Azure, runs locally during development)
builder
    .AddProject<Projects.FaunaFinder_Seeder>("seeder")
    .WithReference(mainDb)
    .WithReference(identityDb)
    .WithReference(wildlifeDb)
    .WaitFor(mainDb)
    .WaitFor(identityDb)
    .WaitFor(wildlifeDb)
    .PublishAsAzureContainerAppJob();

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
