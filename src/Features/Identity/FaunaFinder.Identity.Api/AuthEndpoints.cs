using FaunaFinder.Database.Models.Users;
using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Results.Conflict();
        }

        var isFirstUser = !userManager.Users.Any();

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Status = isFirstUser ? UserStatus.Approved : UserStatus.Pending,
            Role = isFirstUser ? UserRole.Admin : UserRole.Viewer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Results.BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await signInManager.SignInAsync(user, isPersistent: true);

        return Results.Ok(ToUserInfo(user));
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        SignInManager<User> signInManager,
        UserManager<User> userManager)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: request.RememberMe,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return Results.Problem(statusCode: StatusCodes.Status423Locked);
        }

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(ToUserInfo(user));
    }

    private static async Task<IResult> Logout(SignInManager<User> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }

    private static async Task<IResult> GetCurrentUser(
        HttpContext context,
        UserManager<User> userManager)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        var user = await userManager.GetUserAsync(context.User);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(ToUserInfo(user));
    }

    private static UserInfo ToUserInfo(User user) => new(
        user.Id,
        user.Email!,
        user.DisplayName,
        user.Status.ToString(),
        user.Role.ToString()
    );
}
