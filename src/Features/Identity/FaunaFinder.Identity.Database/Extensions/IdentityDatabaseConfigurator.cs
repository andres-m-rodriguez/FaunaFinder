using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FaunaFinder.Identity.Database.Extensions;

public static class IdentityDatabaseConfigurator
{
    public static IHostApplicationBuilder AddIdentityDatabase(
        this IHostApplicationBuilder builder,
        string connectionName = "faunafinder-identity")
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName)
            ?? throw new ArgumentException($"Connection string '{connectionName}' not found");

        // Register DbContext pool
        builder.Services.AddDbContextPool<IdentityDbContext>(options =>
        {
            ConfigureDbContext(connectionString, options);
        });

        // Register pooled factory
        builder.Services.AddPooledDbContextFactory<IdentityDbContext>(options =>
        {
            ConfigureDbContext(connectionString, options);
        });

        // Enrich with Azure-specific configuration (SSL, health checks, telemetry)
        builder.EnrichAzureNpgsqlDbContext<IdentityDbContext>();

        return builder;
    }

    private static void ConfigureDbContext(string connectionString, DbContextOptionsBuilder options)
    {
        options.EnableThreadSafetyChecks(false);
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("FaunaFinder.Identity.Database");
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
        });
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        options.UseSnakeCaseNamingConvention();
    }
}
