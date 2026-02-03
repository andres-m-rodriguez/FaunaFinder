using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Identity.Database.Models;

namespace FaunaFinder.Identity.DataAccess.Interfaces;

public interface IAccessRequestRepository
{
    Task<IReadOnlyList<AccessRequestInfo>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<AccessRequestInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AccessRequest?> GetEntityByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AccessRequestInfo> CreateAsync(AccessRequest accessRequest, CancellationToken cancellationToken = default);
    Task<AccessRequestInfo?> UpdateStatusAsync(int id, AccessRequestStatus status, CancellationToken cancellationToken = default);
}
