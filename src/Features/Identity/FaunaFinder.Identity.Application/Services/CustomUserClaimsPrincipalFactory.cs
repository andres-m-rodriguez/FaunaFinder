using System.Security.Claims;
using FaunaFinder.Identity.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FaunaFinder.Identity.Application.Services;

public sealed class CustomUserClaimsPrincipalFactory(
    UserManager<User> userManager,
    IOptions<IdentityOptions> optionsAccessor
) : UserClaimsPrincipalFactory<User>(userManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
        return identity;
    }
}
