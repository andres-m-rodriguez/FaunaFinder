using FaunaFinder.DataAccess.Interfaces;
using FaunaFinder.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.DataAccess.Extensions;

public static class DataAccessConfigurator
{
    public static IServiceCollection AddFaunaFinderDataAccess(this IServiceCollection services)
    {
        services.AddScoped<IMunicipalityRepository, MunicipalityRepository>();
        services.AddScoped<ISpeciesRepository, SpeciesRepository>();
        services.AddScoped<ISpeciesImageRepository, SpeciesImageRepository>();

        return services;
    }
}
