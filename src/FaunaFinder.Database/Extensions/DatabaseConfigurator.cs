using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FaunaFinder.Database.Extensions;

public static class DatabaseConfigurator
{
    public static IHostApplicationBuilder AddFaunaFinderDatabase(
        this IHostApplicationBuilder builder,
        string connectionName = "faunafinder")
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName)
            ?? throw new ArgumentException($"Connection string '{connectionName}' not found");

        // Register DbContext pool
        builder.Services.AddDbContextPool<FaunaFinderContext>(options =>
        {
            ConfigureDbContext(connectionString, options);
        });

        // Register pooled factory
        builder.Services.AddPooledDbContextFactory<FaunaFinderContext>(options =>
        {
            ConfigureDbContext(connectionString, options);
        });

        // Enrich with Azure-specific configuration (SSL, health checks, telemetry)
        builder.EnrichAzureNpgsqlDbContext<FaunaFinderContext>();

        return builder;
    }

    private static void ConfigureDbContext(string connectionString, DbContextOptionsBuilder options)
    {
        options.EnableThreadSafetyChecks(false);
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("FaunaFinder.Database");
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
            npgsqlOptions.UseNetTopologySuite();
        });
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        options.UseSnakeCaseNamingConvention();
    }
}
