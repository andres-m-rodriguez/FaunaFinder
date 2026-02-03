using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Results;
using FaunaFinder.Pagination.Contracts;

namespace FaunaFinder.Identity.Application.Services;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<GetCurrentUserResult> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<GetUsersResult> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<GetUsersCursorPageResult> GetUsersCursorPageAsync(CursorPageRequest request, CancellationToken cancellationToken = default);
    Task<GetAccessRequestsResult> GetPendingAccessRequestsAsync(CancellationToken cancellationToken = default);
    Task<GetAccessRequestsCursorPageResult> GetAccessRequestsCursorPageAsync(AccessRequestPageRequest request, CancellationToken cancellationToken = default);
    Task<UpdateAccessRequestStatusResult> UpdateAccessRequestStatusAsync(int id, UpdateAccessRequestStatusRequest request, CancellationToken cancellationToken = default);
}
