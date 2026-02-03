using System.Net;
using System.Net.Http.Json;
using FaunaFinder.Identity.Contracts.Errors;
using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Identity.Contracts.Results;
using FluentValidation;

namespace FaunaFinder.Identity.Application.Client;

public sealed class IdentityClient : IIdentityClient
{
    private readonly HttpClient _httpClient;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<UpdateAccessRequestStatusRequest> _updateAccessRequestStatusValidator;

    public IdentityClient(
        HttpClient httpClient,
        IValidator<LoginRequest> loginValidator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<UpdateAccessRequestStatusRequest> updateAccessRequestStatusValidator)
    {
        _httpClient = httpClient;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _updateAccessRequestStatusValidator = updateAccessRequestStatusValidator;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(
                "Validation failed",
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken);
            return user!;
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new InvalidCredentialsError(),
            HttpStatusCode.Locked => new AccountLockedError(null),
            HttpStatusCode.Forbidden => new AccountNotApprovedError(),
            _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken))
        };
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(
                "Validation failed",
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var success = await response.Content.ReadFromJsonAsync<RegisterSuccess>(cancellationToken);
            return success!;
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return new EmailAlreadyExistsError(request.Email);
        }

        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        return new RegistrationFailedError([error]);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("api/auth/logout", null, cancellationToken);
    }

    public async Task<GetCurrentUserResult> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/auth/me", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken);
            return user!;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new UnauthorizedError();
        }

        return new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async Task<GetUsersResult> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/auth/users", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var users = await response.Content.ReadFromJsonAsync<UserInfo[]>(cancellationToken);
            return users!;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new ForbiddenError();

        return new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async Task<GetAccessRequestsResult> GetPendingAccessRequestsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/auth/access-requests/pending", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var requests = await response.Content.ReadFromJsonAsync<AccessRequestInfo[]>(cancellationToken);
            return requests!;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new ForbiddenError();

        return new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async Task<UpdateAccessRequestStatusResult> UpdateAccessRequestStatusAsync(int id, UpdateAccessRequestStatusRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateAccessRequestStatusValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(
                "Validation failed",
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        var response = await _httpClient.PutAsJsonAsync($"api/auth/access-requests/{id}/status", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var accessRequest = await response.Content.ReadFromJsonAsync<AccessRequestInfo>(cancellationToken);
            return accessRequest!;
        }

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => new AccessRequestNotFoundError(id),
            HttpStatusCode.Forbidden => new ForbiddenError(),
            HttpStatusCode.BadRequest => new ValidationError("Validation failed", new Dictionary<string, string[]>()),
            _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken))
        };
    }
}
