using FaunaFinder.Identity.Contracts.Errors;
using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Identity.Contracts.Results;
using FaunaFinder.Identity.Database.Models;
using FaunaFinder.Identity.DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace FaunaFinder.Identity.Application.Services;

public sealed class AuthService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository
) : IAuthService
{
    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return new EmailAlreadyExistsError(request.Email);
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Status = UserStatus.Pending,
            Role = UserRole.Viewer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new RegistrationFailedError(result.Errors.Select(e => e.Description));
        }

        return ToUserInfo(user);
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new InvalidCredentialsError();
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: request.RememberMe,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            var lockoutEnd = await userManager.GetLockoutEndDateAsync(user);
            return new AccountLockedError(lockoutEnd);
        }

        if (!result.Succeeded)
        {
            return new InvalidCredentialsError();
        }

        if (user.Status != UserStatus.Approved)
        {
            await signInManager.SignOutAsync();
            return new AccountNotApprovedError();
        }

        return ToUserInfo(user);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await signInManager.SignOutAsync();
    }

    public async Task<GetCurrentUserResult> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated != true)
        {
            return new UnauthorizedError();
        }

        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return new UnauthorizedError();
        }

        return ToUserInfo(user);
    }

    public async Task<GetUsersResult> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsCurrentUserApprovedAdminAsync())
            return new ForbiddenError();

        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.ToArray();
    }

    public async Task<GetPendingUsersResult> GetPendingUsersAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsCurrentUserApprovedAdminAsync())
            return new ForbiddenError();

        var users = await userRepository.GetPendingUsersAsync(cancellationToken);
        return users.ToArray();
    }

    public async Task<UpdateUserStatusResult> UpdateUserStatusAsync(int userId, UpdateUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (!await IsCurrentUserApprovedAdminAsync())
            return new ForbiddenError();

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null || currentUserId == userId)
            return new ForbiddenError();

        if (!Enum.TryParse<UserStatus>(request.Status, out var newStatus))
            return new ValidationError("Invalid status", new Dictionary<string, string[]>
            {
                ["Status"] = [$"'{request.Status}' is not a valid status"]
            });

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return new UserNotFoundError(userId);

        user.Status = newStatus;
        user.UpdatedAt = DateTime.UtcNow;

        await userManager.UpdateSecurityStampAsync(user);
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return new UnexpectedError(string.Join(", ", result.Errors.Select(e => e.Description)));

        return ToUserInfo(user);
    }

    private int? GetCurrentUserId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        var idClaim = httpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    private async Task<bool> IsCurrentUserApprovedAdminAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated != true)
            return false;

        if (!httpContext.User.IsInRole("Admin"))
            return false;

        var user = await userManager.GetUserAsync(httpContext.User);
        return user is { Status: UserStatus.Approved, Role: UserRole.Admin };
    }

    private static UserInfo ToUserInfo(User user) => new(
        user.Id,
        user.Email!,
        user.DisplayName,
        user.Status.ToString(),
        user.Role.ToString()
    );
}
