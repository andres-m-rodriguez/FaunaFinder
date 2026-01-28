using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Results;

namespace FaunaFinder.Identity.Application.Services;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<GetCurrentUserResult> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
