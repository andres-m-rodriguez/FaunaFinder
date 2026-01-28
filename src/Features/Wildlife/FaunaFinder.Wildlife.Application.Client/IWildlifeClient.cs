using FaunaFinder.Wildlife.Contracts;

namespace FaunaFinder.Wildlife.Application.Client;

public interface IWildlifeClient
{
    Task<IReadOnlyList<SpeciesSearchResult>> SearchSpeciesAsync(string? query, int limit, CancellationToken cancellationToken = default);
    Task<CreateSightingResponse> CreateSightingAsync(CreateSightingRequest request, CancellationToken cancellationToken = default);
    Task<SightingsPage> GetSightingsAsync(int page, int pageSize, string? status = null, CancellationToken cancellationToken = default);
    Task<SightingsPage> GetMySightingsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SightingsPage> GetReviewQueueAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
