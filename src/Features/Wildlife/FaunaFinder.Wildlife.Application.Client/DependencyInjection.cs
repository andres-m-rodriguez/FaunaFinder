using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.Wildlife.Application.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddWildlifeClient(this IServiceCollection services, Action<HttpClient>? configureClient = null)
    {
        services.AddHttpClient<IMunicipalityService, MunicipalityApiService>(client =>
        {
            configureClient?.Invoke(client);
        });

        services.AddHttpClient<ISpeciesService, SpeciesApiService>(client =>
        {
            configureClient?.Invoke(client);
        });

        services.AddHttpClient<IExportService, ExportApiService>(client =>
        {
            configureClient?.Invoke(client);
        });

        services.AddHttpClient<IWildlifeService, WildlifeApiService>(client =>
        {
            configureClient?.Invoke(client);
        });

        return services;
    }

    public static IServiceCollection AddWildlifeClient(this IServiceCollection services, Uri baseAddress)
    {
        return services.AddWildlifeClient(client => client.BaseAddress = baseAddress);
    }
}
