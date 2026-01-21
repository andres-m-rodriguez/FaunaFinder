using FaunaFinder.Core.Interfaces;
using FaunaFinder.Infrastructure.Data;
using FaunaFinder.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFaunaFinderInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IFaunaFinderRepository, FaunaFinderRepository>();
        return services;
    }

    public static IServiceCollection AddFaunaFinderDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<FaunaFinderDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
