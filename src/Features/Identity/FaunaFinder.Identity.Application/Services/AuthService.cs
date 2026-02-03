using FaunaFinder.Identity.Contracts.Errors;
using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Identity.Contracts.Results;
using FaunaFinder.Identity.Database;
using FaunaFinder.Identity.Database.Models;
using FaunaFinder.Identity.DataAccess.Interfaces;
using FaunaFinder.Pagination.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Identity.Application.Services;

public sealed class AuthService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository,
    IAccessRequestRepository accessRequestRepository,
    IPasswordHasher<User> passwordHasher,
    IDbContextFactory<IdentityDbContext> contextFactory
) : IAuthService
{
    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if email already exists as a user
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return new EmailAlreadyExistsError(request.Email);
        }

        // Check if email already has a pending access request
        var existingRequest = await accessRequestRepository.GetEntityByEmailAsync(request.Email, cancellationToken);
        if (existingRequest is not null)
        {
            return new EmailAlreadyExistsError(request.Email);
        }

        // Hash the password
        var hash = passwordHasher.HashPassword(null!, request.Password);

        // Create access request
        var accessRequest = new AccessRequest
        {
            Email = request.Email,
            DisplayName = request.DisplayName,
            PasswordHash = hash,
            Message = request.Message,
            Status = AccessRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await accessRequestRepository.CreateAsync(accessRequest, cancellationToken);

        return new RegisterSuccess("Registration submitted! An admin will review your request.");
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

        return new UserInfo(user.Id, user.Email!, user.DisplayName, user.Role.ToString(), user.CreatedAt);
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

        return new UserInfo(user.Id, user.Email!, user.DisplayName, user.Role.ToString(), user.CreatedAt);
    }

    public async Task<GetUsersResult> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsCurrentUserAdminAsync())
            return new ForbiddenError();

        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.ToArray();
    }

    public async Task<GetUsersCursorPageResult> GetUsersCursorPageAsync(CursorPageRequest request, CancellationToken cancellationToken = default)
    {
        if (!await IsCurrentUserAdminAsync())
            return new ForbiddenError();

        var page = await userRepository.GetCursorPageAsync(request, cancellationToken);
        return page;
    }

    public async Task<GetAccessRequestsResult> GetPendingAccessRequestsAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsCurrentUserAdminAsync())
            return new ForbiddenError();

        var requests = await accessRequestRepository.GetPendingAsync(cancellationToken);
        return requests.ToArray();
    }

    public async Task<UpdateAccessRequestStatusResult> UpdateAccessRequestStatusAsync(int id, UpdateAccessRequestStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (!await IsCurrentUserAdminAsync())
            return new ForbiddenError();

        if (!Enum.TryParse<AccessRequestStatus>(request.Status, out var newStatus))
            return new ValidationError("Invalid status", new Dictionary<string, string[]>
            {
                ["Status"] = [$"'{request.Status}' is not a valid status"]
            });

        // Fetch the access request entity to get password hash for approval
        var accessRequestInfo = await accessRequestRepository.GetByIdAsync(id, cancellationToken);
        if (accessRequestInfo is null)
            return new AccessRequestNotFoundError(id);

        if (newStatus == AccessRequestStatus.Approved)
        {
            // Check if a user with this email already exists
            var existingUser = await userManager.FindByEmailAsync(accessRequestInfo.Email);
            if (existingUser is not null)
            {
                return new ValidationError("User already exists", new Dictionary<string, string[]>
                {
                    ["Email"] = [$"A user with email '{accessRequestInfo.Email}' already exists"]
                });
            }

            // We need the entity to get the password hash
            var accessRequestEntity = await GetAccessRequestEntityAsync(id, cancellationToken);
            if (accessRequestEntity is null)
                return new AccessRequestNotFoundError(id);

            // Create the actual user
            var user = new User
            {
                UserName = accessRequestInfo.Email,
                Email = accessRequestInfo.Email,
                DisplayName = accessRequestInfo.DisplayName,
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return new UnexpectedError(string.Join(", ", createResult.Errors.Select(e => e.Description)));

            // Transfer the password hash directly
            user.PasswordHash = accessRequestEntity.PasswordHash;
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return new UnexpectedError(string.Join(", ", updateResult.Errors.Select(e => e.Description)));
        }

        // Update the access request status
        var updated = await accessRequestRepository.UpdateStatusAsync(id, newStatus, cancellationToken);
        if (updated is null)
            return new AccessRequestNotFoundError(id);

        return updated;
    }

    private async Task<AccessRequest?> GetAccessRequestEntityAsync(int id, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.AccessRequests.FindAsync([id], cancellationToken);
    }

    private async Task<bool> IsCurrentUserAdminAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated != true)
            return false;

        if (!httpContext.User.IsInRole("Admin"))
            return false;

        var user = await userManager.GetUserAsync(httpContext.User);
        return user is { Role: UserRole.Admin };
    }
}
