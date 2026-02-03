using FaunaFinder.Identity.Application.Services;
using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Results;
using FaunaFinder.Pagination.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FaunaFinder.Identity.Api;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Authentication");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapPost("/logout", Logout).RequireAuthorization();
        group.MapGet("/me", GetCurrentUser);

        group.MapGet("/users", GetAllUsers)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/users/search", GetUsersCursorPage)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/access-requests/pending", GetPendingAccessRequests)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
        group.MapPut("/access-requests/{id}/status", UpdateAccessRequestStatus)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return app;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        IValidator<RegisterRequest> validator,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var result = await authService.RegisterAsync(request, cancellationToken);

        return result.Match<IResult>(
            success => Results.Ok(success),
            emailExists => Results.Conflict(emailExists),
            registrationFailed => Results.BadRequest(registrationFailed),
            validationError => Results.BadRequest(validationError),
            unexpected => Results.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IValidator<LoginRequest> validator,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var result = await authService.LoginAsync(request, cancellationToken);

        return result.Match<IResult>(
            userInfo => Results.Ok(userInfo),
            invalidCredentials => Results.Unauthorized(),
            accountLocked => Results.Problem(statusCode: StatusCodes.Status423Locked),
            notApproved => Results.Problem(notApproved.Message, statusCode: StatusCodes.Status403Forbidden),
            validationError => Results.BadRequest(validationError),
            unexpected => Results.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> Logout(
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> GetCurrentUser(
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.GetCurrentUserAsync(cancellationToken);

        return result.Match<IResult>(
            userInfo => Results.Ok(userInfo),
            unauthorized => Results.Unauthorized(),
            unexpected => Results.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> GetAllUsers(
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.GetAllUsersAsync(cancellationToken);

        return result.Match<IResult>(
            users => Results.Ok(users),
            forbidden => Results.Forbid(),
            unexpected => Results.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> GetPendingAccessRequests(
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.GetPendingAccessRequestsAsync(cancellationToken);

        return result.Match<IResult>(
            requests => Results.Ok(requests),
            forbidden => Results.Forbid(),
            unexpected => Results.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> GetUsersCursorPage(
        string? cursor,
        int? pageSize,
        string? search,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var request = new CursorPageRequest(cursor, pageSize ?? 20, search);
        var result = await authService.GetUsersCursorPageAsync(request, cancellationToken);

        return result.Match<IResult>(
            page => Results.Ok(page),
            forbidden => Results.Forbid(),
            unexpected => Results.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> UpdateAccessRequestStatus(
        int id,
        UpdateAccessRequestStatusRequest request,
        IValidator<UpdateAccessRequestStatusRequest> validator,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var result = await authService.UpdateAccessRequestStatusAsync(id, request, cancellationToken);

        return result.Match<IResult>(
            accessRequest => Results.Ok(accessRequest),
            notFound => Results.NotFound(notFound),
            forbidden => Results.Forbid(),
            validationError => Results.BadRequest(validationError),
            unexpected => Results.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }
}
