using FaunaFinder.Database.Extensions;
using FaunaFinder.Identity.Database.Extensions;
using FaunaFinder.Seeder;
using FaunaFinder.Wildlife.Database.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Register all feature databases
// Each feature gets its own isolated database with its own connection string
builder.AddFaunaFinderDatabase();     // Main database (faunafinder)
builder.AddIdentityDatabase();         // Identity feature database (faunafinder-identity)
builder.AddWildlifeDatabase();         // Wildlife feature database (faunafinder-wildlife)

builder.Services.AddHostedService<DatabaseSeederWorker>();

var host = builder.Build();
host.Run();
