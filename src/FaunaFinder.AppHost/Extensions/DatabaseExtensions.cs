using Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FaunaFinder.AppHost.Extensions;

public static class DatabaseExtensions
{
    public static IResourceBuilder<PostgresDatabaseResource> WithDropDatabaseCommand(
        this IResourceBuilder<PostgresDatabaseResource> builder)
    {
        var databaseName = builder.Resource.DatabaseName;

        var commandOptions = new CommandOptions
        {
            UpdateState = context => context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
                ? ResourceCommandState.Enabled
                : ResourceCommandState.Disabled,
            IconName = "Delete",
            IconVariant = IconVariant.Filled,
            ConfirmationMessage = $"Are you sure you want to drop and recreate '{databaseName}'?"
        };

        return builder.WithCommand(
            name: "drop-database",
            displayName: "Drop & Recreate",
            executeCommand: async context =>
            {
                var logger = context.ServiceProvider.GetRequiredService<ILogger<PostgresDatabaseResource>>();
                var connectionString = await builder.Resource.Parent.GetConnectionStringAsync(context.CancellationToken);

                if (string.IsNullOrEmpty(connectionString))
                {
                    return CommandResults.Failure("Could not get connection string");
                }

                try
                {
                    await using var conn = new Npgsql.NpgsqlConnection(connectionString);
                    await conn.OpenAsync(context.CancellationToken);

                    // Terminate existing connections
                    await using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"""
                            SELECT pg_terminate_backend(pg_stat_activity.pid)
                            FROM pg_stat_activity
                            WHERE pg_stat_activity.datname = '{databaseName}'
                            AND pid <> pg_backend_pid();
                            """;
                        await cmd.ExecuteNonQueryAsync(context.CancellationToken);
                    }

                    // Drop database
                    await using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"DROP DATABASE IF EXISTS \"{databaseName}\"";
                        await cmd.ExecuteNonQueryAsync(context.CancellationToken);
                    }

                    // Recreate database
                    await using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"CREATE DATABASE \"{databaseName}\"";
                        await cmd.ExecuteNonQueryAsync(context.CancellationToken);
                    }

                    logger.LogInformation("Database '{DatabaseName}' dropped and recreated", databaseName);
                    return CommandResults.Success();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to drop database '{DatabaseName}'", databaseName);
                    return CommandResults.Failure($"Failed: {ex.Message}");
                }
            },
            commandOptions: commandOptions);
    }
}
