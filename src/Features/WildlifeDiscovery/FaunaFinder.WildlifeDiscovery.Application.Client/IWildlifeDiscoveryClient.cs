using FaunaFinder.WildlifeDiscovery.Contracts.Requests;
using FaunaFinder.WildlifeDiscovery.Contracts.Results;

namespace FaunaFinder.WildlifeDiscovery.Application.Client;

public interface IWildlifeDiscoveryClient
{
    Task<SearchSpeciesResult> SearchSpeciesAsync(string query, int limit = 10, CancellationToken cancellationToken = default);
    Task<CreateSightingResult> CreateSightingAsync(CreateSightingRequest request, CancellationToken cancellationToken = default);
    Task<GetSightingsResult> GetSightingsAsync(int? page = null, int? pageSize = null, string? status = null, CancellationToken cancellationToken = default);
    Task<GetSightingResult> GetSightingAsync(int id, CancellationToken cancellationToken = default);
    Task<GetSightingsResult> GetMySightingsAsync(int? page = null, int? pageSize = null, CancellationToken cancellationToken = default);
    Task<ReviewSightingResult> ReviewSightingAsync(int id, ReviewSightingRequest request, CancellationToken cancellationToken = default);
    Task<CreateUserSpeciesResult> CreateUserSpeciesAsync(CreateUserSpeciesRequest request, CancellationToken cancellationToken = default);
    Task<GetUserSpeciesResult> GetUserSpeciesAsync(int id, CancellationToken cancellationToken = default);
    Task<VerifyUserSpeciesResult> VerifyUserSpeciesAsync(int id, VerifyUserSpeciesRequest request, CancellationToken cancellationToken = default);
    Task<GetReviewQueueResult> GetReviewQueueAsync(CancellationToken cancellationToken = default);
    string GetSightingPhotoUrl(int sightingId);
    string GetSightingAudioUrl(int sightingId);
    string GetUserSpeciesPhotoUrl(int userSpeciesId);
}
