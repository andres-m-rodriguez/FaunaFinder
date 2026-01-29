using FaunaFinder.Wildlife.DataAccess.Interfaces;
using FaunaFinder.Wildlife.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.Wildlife.DataAccess.Extensions;

public static class WildlifeDataAccessConfigurator
{
    public static IServiceCollection AddWildlifeDataAccess(this IServiceCollection services)
    {
        services.AddScoped<IMunicipalityRepository, MunicipalityRepository>();
        services.AddScoped<ISpeciesRepository, SpeciesRepository>();
        services.AddScoped<ISpeciesImageRepository, SpeciesImageRepository>();
        services.AddScoped<ISightingRepository, SightingRepository>();

        return services;
    }
}
