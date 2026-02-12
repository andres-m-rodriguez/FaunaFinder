using FaunaFinder.Identity.Application.Services;
using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Pagination.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FaunaFinder.Identity.Api;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Authentication");

        // Public endpoints
        group.MapPost("/register", Register).WithName("Register");
        group.MapPost("/login", Login).WithName("Login");
        group.MapGet("/me", GetCurrentUser).WithName("GetCurrentUser");

        // Authenticated endpoints
        group.MapPost("/logout", Logout).RequireAuthorization().WithName("Logout");

        // Admin endpoints
        group.MapGet("/users", GetAllUsers).RequireAuthorization(p => p.RequireRole("Admin")).WithName("GetAllUsers");
        group.MapGet("/users/search", GetUsersCursorPage).RequireAuthorization(p => p.RequireRole("Admin")).WithName("GetUsersCursorPage");
        group.MapGet("/access-requests/pending", GetPendingAccessRequests).RequireAuthorization(p => p.RequireRole("Admin")).WithName("GetPendingAccessRequests");
        group.MapGet("/access-requests/search", GetAccessRequestsCursorPage).RequireAuthorization(p => p.RequireRole("Admin")).WithName("GetAccessRequestsCursorPage");
        group.MapGet("/access-requests/{id:int}", GetAccessRequestById).RequireAuthorization(p => p.RequireRole("Admin")).WithName("GetAccessRequestById");
        group.MapPut("/access-requests/{id:int}/status", UpdateAccessRequestStatus).RequireAuthorization(p => p.RequireRole("Admin")).WithName("UpdateAccessRequestStatus");

        return app;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        IValidator<RegisterRequest> validator,
        IAuthService authService,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return TypedResults.ValidationProblem(validation.ToDictionary());

        var result = await authService.RegisterAsync(request, ct);

        return result.Match<IResult>(
            success => TypedResults.Ok(success),
            emailExists => TypedResults.Conflict(emailExists),
            registrationFailed => TypedResults.BadRequest(registrationFailed),
            validationError => TypedResults.BadRequest(validationError),
            unexpected => TypedResults.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IValidator<LoginRequest> validator,
        IAuthService authService,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return TypedResults.ValidationProblem(validation.ToDictionary());

        var result = await authService.LoginAsync(request, ct);

        return result.Match<IResult>(
            userInfo => TypedResults.Ok(userInfo),
            invalidCredentials => TypedResults.Unauthorized(),
            accountLocked => TypedResults.Problem(statusCode: StatusCodes.Status423Locked),
            notApproved => TypedResults.Problem(notApproved.Message, statusCode: StatusCodes.Status403Forbidden),
            validationError => TypedResults.BadRequest(validationError),
            unexpected => TypedResults.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<Ok> Logout(
        IAuthService authService,
        CancellationToken ct)
    {
        await authService.LogoutAsync(ct);
        return TypedResults.Ok();
    }

    private static async Task<IResult> GetCurrentUser(
        IAuthService authService,
        CancellationToken ct)
    {
        var result = await authService.GetCurrentUserAsync(ct);

        return result.Match<IResult>(
            userInfo => TypedResults.Ok(userInfo),
            unauthorized => TypedResults.Unauthorized(),
            unexpected => TypedResults.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> GetAllUsers(
        IAuthService authService,
        CancellationToken ct)
    {
        var result = await authService.GetAllUsersAsync(ct);

        return result.Match<IResult>(
            users => TypedResults.Ok(users),
            forbidden => TypedResults.Forbid(),
            unexpected => TypedResults.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> GetPendingAccessRequests(
        IAuthService authService,
        CancellationToken ct)
    {
        var result = await authService.GetPendingAccessRequestsAsync(ct);

        return result.Match<IResult>(
            requests => TypedResults.Ok(requests),
            forbidden => TypedResults.Forbid(),
            unexpected => TypedResults.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> GetAccessRequestById(
        int id,
        IAuthService authService,
        CancellationToken ct)
    {
        var result = await authService.GetAccessRequestByIdAsync(id, ct);

        return result.Match<IResult>(
            accessRequest => TypedResults.Ok(accessRequest),
            notFound => TypedResults.NotFound(notFound),
            forbidden => TypedResults.Forbid(),
            unexpected => TypedResults.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> GetUsersCursorPage(
        [AsParameters] CursorPageParameter parameters,
        IAuthService authService,
        CancellationToken ct)
    {
        var result = await authService.GetUsersCursorPageAsync(parameters, ct);

        return result.Match<IResult>(
            page => TypedResults.Ok(page),
            forbidden => TypedResults.Forbid(),
            unexpected => TypedResults.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> GetAccessRequestsCursorPage(
        [AsParameters] AccessRequestPageParameter parameters,
        IAuthService authService,
        CancellationToken ct)
    {
        var result = await authService.GetAccessRequestsCursorPageAsync(parameters, ct);

        return result.Match<IResult>(
            page => TypedResults.Ok(page),
            forbidden => TypedResults.Forbid(),
            unexpected => TypedResults.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> UpdateAccessRequestStatus(
        int id,
        UpdateAccessRequestStatusRequest request,
        IValidator<UpdateAccessRequestStatusRequest> validator,
        IAuthService authService,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return TypedResults.ValidationProblem(validation.ToDictionary());

        var result = await authService.UpdateAccessRequestStatusAsync(id, request, ct);

        return result.Match<IResult>(
            accessRequest => TypedResults.Ok(accessRequest),
            notFound => TypedResults.NotFound(notFound),
            forbidden => TypedResults.Forbid(),
            validationError => TypedResults.BadRequest(validationError),
            unexpected => TypedResults.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }
}
