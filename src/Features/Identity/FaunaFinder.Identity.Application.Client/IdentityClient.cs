using System.Net;
using System.Net.Http.Json;
using FaunaFinder.Identity.Contracts.Errors;
using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Identity.Contracts.Results;
using FaunaFinder.Pagination.Contracts;
using FluentValidation;

namespace FaunaFinder.Identity.Application.Client;

public sealed class IdentityClient(
    HttpClient httpClient,
    IValidator<LoginRequest> loginValidator,
    IValidator<RegisterRequest> registerValidator,
    IValidator<UpdateAccessRequestStatusRequest> updateAccessRequestStatusValidator) : IIdentityClient
{
    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(
                "Validation failed",
                validationResult
                    .Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            );
        }

        var response = await httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);

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
            HttpStatusCode.TooManyRequests => ParseTooManyRequestsError(response),
            _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken)),
        };
    }

    private static TooManyRequestsError ParseTooManyRequestsError(HttpResponseMessage response)
    {
        int? retryAfter = null;
        if (response.Headers.RetryAfter?.Delta is { } delta)
        {
            retryAfter = (int)delta.TotalSeconds;
        }
        return new TooManyRequestsError(retryAfter);
    }

    public async Task<RegisterResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(
                "Validation failed",
                validationResult
                    .Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            );
        }

        var response = await httpClient.PostAsJsonAsync("api/auth/register", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var success = await response.Content.ReadFromJsonAsync<RegisterSuccess>(
                cancellationToken
            );
            return success!;
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return new EmailAlreadyExistsError(request.Email);
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return ParseTooManyRequestsError(response);
        }

        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        return new RegistrationFailedError([error]);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await httpClient.PostAsync("api/auth/logout", null, cancellationToken);
    }

    public async Task<GetCurrentUserResult> GetCurrentUserAsync(
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.GetAsync("api/auth/me", cancellationToken);

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

    public async Task<GetUsersResult> GetAllUsersAsync(
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.GetAsync("api/auth/users", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var users = await response.Content.ReadFromJsonAsync<UserInfo[]>(cancellationToken);
            return users!;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new ForbiddenError();

        return new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async Task<GetUsersCursorPageResult> GetUsersCursorPageAsync(
        CursorPageParameter request,
        CancellationToken cancellationToken = default
    )
    {
        var queryParams = new List<string> { $"pageSize={request.PageSize}" };

        if (!string.IsNullOrEmpty(request.Cursor))
            queryParams.Add($"cursor={Uri.EscapeDataString(request.Cursor)}");

        if (!string.IsNullOrEmpty(request.Search))
            queryParams.Add($"search={Uri.EscapeDataString(request.Search)}");

        var url = $"api/auth/users/search?{string.Join("&", queryParams)}";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var page = await response.Content.ReadFromJsonAsync<CursorPage<UserInfo>>(
                cancellationToken
            );
            return page!;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new ForbiddenError();

        return new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async Task<GetAccessRequestsCursorPageResult> GetAccessRequestsCursorPageAsync(
        AccessRequestPageParameter request,
        CancellationToken cancellationToken = default
    )
    {
        var queryParams = new List<string> { $"pageSize={request.PageSize}" };

        if (!string.IsNullOrEmpty(request.Cursor))
            queryParams.Add($"cursor={Uri.EscapeDataString(request.Cursor)}");

        if (!string.IsNullOrEmpty(request.Search))
            queryParams.Add($"search={Uri.EscapeDataString(request.Search)}");

        if (!string.IsNullOrEmpty(request.Status))
            queryParams.Add($"status={Uri.EscapeDataString(request.Status)}");

        var url = $"api/auth/access-requests/search?{string.Join("&", queryParams)}";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var page = await response.Content.ReadFromJsonAsync<CursorPage<AccessRequestInfo>>(
                cancellationToken
            );
            return page!;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new ForbiddenError();

        return new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async Task<GetAccessRequestByIdResult> GetAccessRequestByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.GetAsync($"api/auth/access-requests/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var accessRequest = await response.Content.ReadFromJsonAsync<AccessRequestInfo>(
                cancellationToken
            );
            return accessRequest!;
        }

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => new AccessRequestNotFoundError(id),
            HttpStatusCode.Forbidden => new ForbiddenError(),
            _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken)),
        };
    }

    public async Task<GetAccessRequestsResult> GetPendingAccessRequestsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var response = await httpClient.GetAsync("api/auth/access-requests/pending", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var requests = await response.Content.ReadFromJsonAsync<AccessRequestInfo[]>(
                cancellationToken
            );
            return requests!;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new ForbiddenError();

        return new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async Task<UpdateAccessRequestStatusResult> UpdateAccessRequestStatusAsync(
        int id,
        UpdateAccessRequestStatusRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await updateAccessRequestStatusValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(
                "Validation failed",
                validationResult
                    .Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            );
        }

        var response = await httpClient.PutAsJsonAsync($"api/auth/access-requests/{id}/status", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var accessRequest = await response.Content.ReadFromJsonAsync<AccessRequestInfo>(
                cancellationToken
            );
            return accessRequest!;
        }

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => new AccessRequestNotFoundError(id),
            HttpStatusCode.Forbidden => new ForbiddenError(),
            HttpStatusCode.BadRequest => new ValidationError(
                "Validation failed",
                new Dictionary<string, string[]>()
            ),
            _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken)),
        };
    }
}
