using Azure.Provisioning.AppContainers;
using Azure.Provisioning.ContainerRegistry;

var builder = DistributedApplication.CreateBuilder(args);


var acr = builder.AddAzureContainerRegistry("acr");

builder.AddAzureContainerAppEnvironment("aca")
    .WithAzureContainerRegistry(acr);

var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(configureContainer: container =>
    {
        container.WithImage("postgis/postgis", "17-3.5");
        container.WithDataVolume("faunafinder-postgres-data");
        container.WithPgAdmin();
    });

var identityDb = postgres.AddDatabase("faunafinder-identity");
var wildlifeDb = postgres.AddDatabase("faunafinder-wildlife");
var mainDb = postgres.AddDatabase("faunafinder");

// Database seeder (runs first, seeds all databases)
var seeder = builder.AddProject<Projects.FaunaFinder_Seeder>("seeder")
    .WithReference(mainDb)
    .WithReference(identityDb)
    .WithReference(wildlifeDb)
    .WaitFor(mainDb)
    .WaitFor(identityDb)
    .WaitFor(wildlifeDb);

// API + WASM Client
builder.AddProject<Projects.FaunaFinder_Server>("server")
    .WithReference(mainDb)
    .WithReference(identityDb)
    .WithReference(wildlifeDb)
    .WaitFor(seeder)
    .WithExternalHttpEndpoints();

builder.Build().Run();
