using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.Wildlife.Application.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddWildlifeClient(this IServiceCollection services, Action<HttpClient>? configureClient = null)
    {
        services.AddHttpClient<IMunicipalityHttpClient, MunicipalityHttpClient>(client =>
        {
            configureClient?.Invoke(client);
        });

        services.AddHttpClient<ISpeciesHttpClient, SpeciesHttpClient>(client =>
        {
            configureClient?.Invoke(client);
        });

        services.AddHttpClient<IExportHttpClient, ExportHttpClient>(client =>
        {
            configureClient?.Invoke(client);
        });

        services.AddHttpClient<IWildlifeHttpClient, WildlifeHttpClient>(client =>
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
