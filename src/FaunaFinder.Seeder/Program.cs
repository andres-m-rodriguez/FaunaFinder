using FaunaFinder.Database.Extensions;
using FaunaFinder.Seeder;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddFaunaFinderDatabase();

builder.Services.AddHostedService<DatabaseSeederWorker>();

var host = builder.Build();
host.Run();
