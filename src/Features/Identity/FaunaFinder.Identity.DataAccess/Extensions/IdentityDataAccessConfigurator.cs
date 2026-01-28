using FaunaFinder.Identity.DataAccess.Interfaces;
using FaunaFinder.Identity.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.Identity.DataAccess.Extensions;

public static class IdentityDataAccessConfigurator
{
    public static IServiceCollection AddIdentityDataAccess(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
