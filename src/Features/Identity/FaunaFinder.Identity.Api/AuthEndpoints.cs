using FaunaFinder.Identity.Application.Services;
using FaunaFinder.Identity.Contracts.Errors;
using FaunaFinder.Identity.Contracts.Requests;
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

        return app;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
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
        IAuthService authService,
        CancellationToken cancellationToken)
    {
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
}
