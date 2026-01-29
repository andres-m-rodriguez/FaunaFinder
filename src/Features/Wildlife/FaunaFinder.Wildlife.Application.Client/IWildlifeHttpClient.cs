using FaunaFinder.Wildlife.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;

namespace FaunaFinder.Wildlife.Application.Client;

public interface IWildlifeHttpClient
{
    Task<IReadOnlyList<SpeciesSearchResult>> SearchSpeciesAsync(
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default);

    Task<SightingsPage> GetMySightingsAsync(
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);

    Task<SightingsPage> GetSightingsAsync(
        int page,
        int pageSize,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<SightingsPage> GetReviewQueueAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CreateSightingResponse> CreateSightingAsync(
        CreateSightingRequest request,
        CancellationToken cancellationToken = default);

    Task<SightingDetailDto?> GetSightingDetailAsync(
        int id,
        CancellationToken cancellationToken = default);
}
