using FaunaFinder.Identity.Application.Services;
using FaunaFinder.Identity.Database;
using FaunaFinder.Identity.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FaunaFinder.Identity.Application.Extensions;

public static class IdentityApplicationConfigurator
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        // Add ASP.NET Core Identity
        services.AddIdentity<User, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

        // Revalidate the security stamp frequently so that status/role
        // changes invalidate existing sessions quickly
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromMinutes(1);
        });

        // Add application services
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
