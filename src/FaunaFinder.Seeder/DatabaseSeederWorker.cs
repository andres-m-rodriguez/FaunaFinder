using FaunaFinder.Database;
using FaunaFinder.Identity.Database;
using FaunaFinder.Wildlife.Database;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Seeder;

public sealed class DatabaseSeederWorker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime lifetime,
    ILogger<DatabaseSeederWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            // Apply migrations and seed all databases
            await MigrateAndSeedMainDatabaseAsync(scope.ServiceProvider, stoppingToken);
            await MigrateFeatureDatabaseAsync<IdentityDbContext>(scope.ServiceProvider, "Identity", stoppingToken);
            await MigrateFeatureDatabaseAsync<WildlifeDbContext>(scope.ServiceProvider, "Wildlife", stoppingToken);

            logger.LogInformation("All database migrations and seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the databases.");
            throw;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }

    private async Task MigrateAndSeedMainDatabaseAsync(IServiceProvider services, CancellationToken stoppingToken)
    {
        var context = services.GetRequiredService<FaunaFinderContext>();

        logger.LogInformation("Applying main database migrations...");
        await context.Database.MigrateAsync(stoppingToken);

        logger.LogInformation("Seeding main database...");
        await DatabaseSeeder.SeedAsync(context, stoppingToken);
    }

    private async Task MigrateFeatureDatabaseAsync<TContext>(
        IServiceProvider services,
        string featureName,
        CancellationToken stoppingToken) where TContext : DbContext
    {
        var context = services.GetRequiredService<TContext>();

        logger.LogInformation("Applying {Feature} database migrations...", featureName);
        await context.Database.MigrateAsync(stoppingToken);
        logger.LogInformation("{Feature} database migrations applied.", featureName);
    }
}
