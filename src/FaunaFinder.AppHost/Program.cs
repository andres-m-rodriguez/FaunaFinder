var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL database
// - Uses container locally for development
// - Uses Azure PostgreSQL Flexible Server when deployed to Azure
var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(configureContainer: container =>
    {
        container.WithImage("postgis/postgis", "17-3.5");
        container.WithDataVolume("faunafinder-postgres-data");
        container.WithPgAdmin();
    });

var db = postgres.AddDatabase("faunafinder");

// Database seeder (runs first)
var seeder = builder.AddProject<Projects.FaunaFinder_Seeder>("seeder")
    .WithReference(db)
    .WaitFor(db);

// API + WASM Client
builder.AddProject<Projects.FaunaFinder_Api>("api")
    .WithReference(db)
    .WaitFor(seeder)
    .WithExternalHttpEndpoints();

builder.Build().Run();
