var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL database
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("faunafinder-postgres-data")
    .WithPgAdmin();

var db = postgres.AddDatabase("faunafinder");

// Database seeder (runs first)
var seeder = builder.AddProject<Projects.FaunaFinder_Seeder>("seeder")
    .WithReference(db)
    .WaitFor(db);

// API + Blazor Server app
builder.AddProject<Projects.FaunaFinder_Api>("api")
    .WithReference(db)
    .WaitFor(seeder)
    .WithExternalHttpEndpoints();

builder.Build().Run();
