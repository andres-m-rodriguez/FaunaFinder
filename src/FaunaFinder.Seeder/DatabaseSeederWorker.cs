using FaunaFinder.Database;
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
            var context = scope.ServiceProvider.GetRequiredService<FaunaFinderContext>();

            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync(stoppingToken);

            logger.LogInformation("Seeding database...");
            await DatabaseSeeder.SeedAsync(context, stoppingToken);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }
}
