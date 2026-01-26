using System.Security.Claims;
using FaunaFinder.Identity.Application.Client;
using FaunaFinder.Identity.Contracts.Errors;
using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Identity.Contracts.Results;
using Microsoft.AspNetCore.Components.Authorization;

namespace FaunaFinder.Client.Services.Auth;

public class CookieAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IIdentityClient _identityClient;
    private UserInfo? _cachedUser;
    private bool _isInitialized;

    public CookieAuthenticationStateProvider(IIdentityClient identityClient)
    {
        _identityClient = identityClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_isInitialized)
        {
            var result = await _identityClient.GetCurrentUserAsync();
            _cachedUser = result.Match<UserInfo?>(
                user => user,
                _ => null,
                _ => null);
            _isInitialized = true;
        }

        return CreateAuthenticationState(_cachedUser);
    }

    public async Task<LoginResult> LoginAsync(string email, string password, bool rememberMe = false)
    {
        var result = await _identityClient.LoginAsync(new LoginRequest(email, password, rememberMe));

        result.Switch(
            user =>
            {
                _cachedUser = user;
                NotifyAuthenticationStateChanged(Task.FromResult(CreateAuthenticationState(_cachedUser)));
            },
            _ => { },
            _ => { },
            _ => { },
            _ => { },
            _ => { });

        return result;
    }

    public async Task<RegisterResult> RegisterAsync(string email, string password, string displayName)
    {
        var result = await _identityClient.RegisterAsync(new RegisterRequest(email, password, displayName));

        result.Switch(
            user =>
            {
                _cachedUser = user;
                NotifyAuthenticationStateChanged(Task.FromResult(CreateAuthenticationState(_cachedUser)));
            },
            _ => { },
            _ => { },
            _ => { },
            _ => { });

        return result;
    }

    public async Task LogoutAsync()
    {
        await _identityClient.LogoutAsync();
        _cachedUser = null;
        NotifyAuthenticationStateChanged(Task.FromResult(CreateAuthenticationState(null)));
    }

    public UserInfo? CurrentUser => _cachedUser;

    private static AuthenticationState CreateAuthenticationState(UserInfo? user)
    {
        if (user is null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new("status", user.Status),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, "cookie");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}
