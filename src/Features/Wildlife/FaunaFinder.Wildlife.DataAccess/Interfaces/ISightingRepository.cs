using FaunaFinder.Wildlife.Contracts;

namespace FaunaFinder.Wildlife.DataAccess.Interfaces;

public interface ISightingRepository
{
    Task<SightingsPage> GetSightingsAsync(int page, int pageSize, string? status, CancellationToken cancellationToken = default);
    Task<SightingsPage> GetSightingsByUserAsync(int userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SightingsPage> GetReviewQueueAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SightingListItem?> GetSightingAsync(int id, CancellationToken cancellationToken = default);
}
