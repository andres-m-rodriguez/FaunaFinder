using FaunaFinder.Identity.Application.Services;
using FaunaFinder.Identity.Contracts.Errors;
using FaunaFinder.Identity.Contracts.Requests;
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
        group.MapGet("/users/pending", GetPendingUsers)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
        group.MapPut("/users/{id}/status", UpdateUserStatus)
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
            userInfo => Results.Ok(userInfo),
            emailExists => Results.Conflict(emailExists),
            registrationFailed => Results.BadRequest(registrationFailed),
            validation => Results.BadRequest(validation),
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
            validation => Results.BadRequest(validation),
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

    private static async Task<IResult> GetPendingUsers(
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.GetPendingUsersAsync(cancellationToken);

        return result.Match<IResult>(
            users => Results.Ok(users),
            forbidden => Results.Forbid(),
            unexpected => Results.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    private static async Task<IResult> UpdateUserStatus(
        int id,
        UpdateUserStatusRequest request,
        IValidator<UpdateUserStatusRequest> validator,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var result = await authService.UpdateUserStatusAsync(id, request, cancellationToken);

        return result.Match<IResult>(
            userInfo => Results.Ok(userInfo),
            notFound => Results.NotFound(notFound),
            forbidden => Results.Forbid(),
            validation => Results.BadRequest(validation),
            unexpected => Results.Problem(unexpected.Message, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

}
