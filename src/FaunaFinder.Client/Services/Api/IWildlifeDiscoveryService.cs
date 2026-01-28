using FaunaFinder.WildlifeDiscovery.Contracts;

namespace FaunaFinder.Client.Services.Api;

public interface IWildlifeDiscoveryService
{
    Task<IReadOnlyList<SpeciesSearchResult>> SearchSpeciesAsync(
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default);

    Task<SightingsPage> GetMySightingsAsync(
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);

    Task<CreateSightingResponse> CreateSightingAsync(
        CreateSightingRequest request,
        CancellationToken cancellationToken = default);
}
