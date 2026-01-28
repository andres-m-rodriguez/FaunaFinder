using FaunaFinder.Identity.Contracts.Responses;

namespace FaunaFinder.Identity.DataAccess.Interfaces;

public interface IUserRepository
{
    Task<UserInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserInfo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserInfo>> GetPendingUsersAsync(CancellationToken cancellationToken = default);
}
