using FaunaFinder.Wildlife.Application.Services;
using FaunaFinder.Wildlife.DataAccess.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.Wildlife.Application.Extensions;

public static class WildlifeApplicationConfigurator
{
    public static IServiceCollection AddWildlifeApplication(this IServiceCollection services)
    {
        // Add data access layer
        services.AddWildlifeDataAccess();

        // Add application services
        services.AddScoped<IWildlifeService, WildlifeService>();

        return services;
    }
}
