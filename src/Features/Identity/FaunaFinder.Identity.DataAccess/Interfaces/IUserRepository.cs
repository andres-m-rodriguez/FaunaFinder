using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Pagination.Contracts;

namespace FaunaFinder.Identity.DataAccess.Interfaces;

public interface IUserRepository
{
    Task<UserInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserInfo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CursorPage<UserInfo>> GetCursorPageAsync(CursorPageParameter request, CancellationToken cancellationToken = default);
}
