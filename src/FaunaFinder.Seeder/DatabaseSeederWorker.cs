using FaunaFinder.Identity.Database;
using FaunaFinder.Wildlife.Database;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Seeder;

public sealed class DatabaseSeederWorker(
    IServiceProvider serviceProvider,
    IHostEnvironment environment,
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
            await MigrateAndSeedIdentityDatabaseAsync(scope.ServiceProvider, stoppingToken);
            await MigrateAndSeedWildlifeDatabaseAsync(scope.ServiceProvider, stoppingToken);

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

    private async Task MigrateAndSeedWildlifeDatabaseAsync(IServiceProvider services, CancellationToken stoppingToken)
    {
        var context = services.GetRequiredService<WildlifeDbContext>();

        logger.LogInformation("Applying Wildlife database migrations...");
        await context.Database.MigrateAsync(stoppingToken);

        logger.LogInformation("Seeding Wildlife database...");
        await DatabaseSeeder.SeedAsync(context, stoppingToken);
        logger.LogInformation("Wildlife database seeding completed.");
    }

    private async Task MigrateAndSeedIdentityDatabaseAsync(IServiceProvider services, CancellationToken stoppingToken)
    {
        var context = services.GetRequiredService<IdentityDbContext>();

        logger.LogInformation("Applying Identity database migrations...");
        await context.Database.MigrateAsync(stoppingToken);
        logger.LogInformation("Identity database migrations applied.");

        if (environment.IsDevelopment())
        {
            logger.LogInformation("Seeding admin user (development only)...");
            await IdentitySeeder.SeedAsync(context, stoppingToken);
            logger.LogInformation("Identity database seeding completed.");
        }
    }
}
