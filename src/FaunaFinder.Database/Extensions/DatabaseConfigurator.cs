using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FaunaFinder.Database.Extensions;

public static class DatabaseConfigurator
{
    public static IHostApplicationBuilder AddFaunaFinderDatabase(
        this IHostApplicationBuilder builder,
        string connectionName = "faunafinder")
    {
        builder.AddNpgsqlDbContext<FaunaFinderContext>(connectionName, configureDbContextOptions: options =>
        {
            options.UseSnakeCaseNamingConvention();
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        builder.Services.AddPooledDbContextFactory<FaunaFinderContext>(options =>
        {
            // The connection string will be configured by Aspire
        });

        return builder;
    }
}
