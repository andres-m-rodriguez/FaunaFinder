using FaunaFinder.Identity.Contracts.Requests;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.Identity.Application.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityClient(this IServiceCollection services, Action<HttpClient>? configureClient = null)
    {
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();

        services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
        {
            configureClient?.Invoke(client);
        });

        return services;
    }

    public static IServiceCollection AddIdentityClient(this IServiceCollection services, Uri baseAddress)
    {
        return services.AddIdentityClient(client => client.BaseAddress = baseAddress);
    }
}
