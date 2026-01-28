using FaunaFinder.Wildlife.DataAccess.Interfaces;
using FaunaFinder.Wildlife.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.Wildlife.DataAccess.Extensions;

public static class WildlifeDataAccessConfigurator
{
    public static IServiceCollection AddWildlifeDataAccess(this IServiceCollection services)
    {
        services.AddScoped<ISightingRepository, SightingRepository>();
        services.AddScoped<ISpeciesRepository, SpeciesRepository>();

        return services;
    }
}
