var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL database server
// - Uses container locally for development
// - Uses Azure PostgreSQL Flexible Server when deployed to Azure
var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(configureContainer: container =>
    {
        container.WithImage("postgis/postgis", "17-3.5");
        container.WithDataVolume("faunafinder-postgres-data");
        container.WithPgAdmin();
    });

// Feature-specific databases
// Each feature module gets its own isolated database
var identityDb = postgres.AddDatabase("faunafinder-identity");
var wildlifeDb = postgres.AddDatabase("faunafinder-wildlife");

// Main database (for shared/legacy tables during transition)
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
builder.AddProject<Projects.FaunaFinder_Api>("api")
    .WithReference(mainDb)
    .WithReference(identityDb)
    .WithReference(wildlifeDb)
    .WaitFor(seeder)
    .WithExternalHttpEndpoints();

builder.Build().Run();
