using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FaunaFinder.Wildlife.Database.Extensions;

public static class WildlifeDatabaseConfigurator
{
    public static IHostApplicationBuilder AddWildlifeDatabase(
        this IHostApplicationBuilder builder,
        string connectionName = "faunafinder-wildlife")
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName)
            ?? throw new ArgumentException($"Connection string '{connectionName}' not found");

        // Register DbContext pool
        builder.Services.AddDbContextPool<WildlifeDbContext>(options =>
        {
            ConfigureDbContext(connectionString, options);
        });

        // Register pooled factory
        builder.Services.AddPooledDbContextFactory<WildlifeDbContext>(options =>
        {
            ConfigureDbContext(connectionString, options);
        });

        // Enrich with Azure-specific configuration (SSL, health checks, telemetry)
        builder.EnrichAzureNpgsqlDbContext<WildlifeDbContext>();

        return builder;
    }

    private static void ConfigureDbContext(string connectionString, DbContextOptionsBuilder options)
    {
        options.EnableThreadSafetyChecks(false);
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("FaunaFinder.Wildlife.Database");
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
            npgsqlOptions.UseNetTopologySuite();
        });
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        options.UseSnakeCaseNamingConvention();
    }
}
