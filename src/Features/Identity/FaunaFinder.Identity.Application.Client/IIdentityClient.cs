using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Results;

namespace FaunaFinder.Identity.Application.Client;

public interface IIdentityClient
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<GetCurrentUserResult> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<GetUsersResult> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<GetPendingUsersResult> GetPendingUsersAsync(CancellationToken cancellationToken = default);
    Task<UpdateUserStatusResult> UpdateUserStatusAsync(int userId, UpdateUserStatusRequest request, CancellationToken cancellationToken = default);
}
