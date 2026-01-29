using FluentValidation;
using FaunaFinder.Wildlife.Contracts.Validators;
using FaunaFinder.Wildlife.DataAccess.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.Wildlife.Application.Extensions;

public static class WildlifeApplicationConfigurator
{
    public static IServiceCollection AddWildlifeApplication(this IServiceCollection services)
    {
        // Add data access layer
        services.AddWildlifeDataAccess();

        // Register validators from Contracts assembly
        services.AddValidatorsFromAssemblyContaining<CreateSightingRequestValidator>();

        return services;
    }
}
