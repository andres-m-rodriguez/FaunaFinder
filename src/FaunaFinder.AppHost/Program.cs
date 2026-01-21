var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL database
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("faunafinder-postgres-data")
    .WithPgAdmin();

var db = postgres.AddDatabase("faunafinder");

// API + Blazor Server app
builder.AddProject<Projects.FaunaFinder_Api>("api")
    .WithReference(db)
    .WaitFor(db)
    .WithExternalHttpEndpoints();

builder.Build().Run();
