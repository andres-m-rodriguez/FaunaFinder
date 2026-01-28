using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.Wildlife.Application.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddWildlifeClient(this IServiceCollection services)
    {
        services.AddScoped<IWildlifeClient, WildlifeClient>();
        return services;
    }
}
