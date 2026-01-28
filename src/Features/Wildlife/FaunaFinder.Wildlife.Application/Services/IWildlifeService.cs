using FaunaFinder.Wildlife.Contracts;

namespace FaunaFinder.Wildlife.Application.Services;

public interface IWildlifeService
{
    Task<IReadOnlyList<SpeciesSearchResult>> SearchSpeciesAsync(string? query, int limit, CancellationToken cancellationToken = default);
    Task<CreateSightingResponse> CreateSightingAsync(CreateSightingRequest request, int userId, CancellationToken cancellationToken = default);
    Task<SightingsPage> GetSightingsAsync(int page, int pageSize, string? status, CancellationToken cancellationToken = default);
    Task<SightingsPage> GetMySightingsAsync(int userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SightingsPage> GetReviewQueueAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> ReviewSightingAsync(int sightingId, int reviewerUserId, string status, string? reviewNotes, CancellationToken cancellationToken = default);
    Task<byte[]?> GetSightingPhotoAsync(int sightingId, CancellationToken cancellationToken = default);
}
