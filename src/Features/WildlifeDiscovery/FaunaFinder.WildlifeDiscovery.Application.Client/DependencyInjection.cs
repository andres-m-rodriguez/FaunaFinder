using FaunaFinder.WildlifeDiscovery.Contracts.Requests;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.WildlifeDiscovery.Application.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddWildlifeDiscoveryClient(this IServiceCollection services, Action<HttpClient>? configureClient = null)
    {
        services.AddScoped<IValidator<CreateSightingRequest>, CreateSightingRequestValidator>();
        services.AddScoped<IValidator<CreateUserSpeciesRequest>, CreateUserSpeciesRequestValidator>();
        services.AddScoped<IValidator<ReviewSightingRequest>, ReviewSightingRequestValidator>();
        services.AddScoped<IValidator<VerifyUserSpeciesRequest>, VerifyUserSpeciesRequestValidator>();

        services.AddHttpClient<IWildlifeDiscoveryClient, WildlifeDiscoveryClient>(client =>
        {
            configureClient?.Invoke(client);
        });

        return services;
    }

    public static IServiceCollection AddWildlifeDiscoveryClient(this IServiceCollection services, Uri baseAddress)
    {
        return services.AddWildlifeDiscoveryClient(client => client.BaseAddress = baseAddress);
    }
}
