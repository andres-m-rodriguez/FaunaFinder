using FaunaFinder.Wildlife.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Wildlife.Contracts.Requests;

namespace FaunaFinder.Wildlife.DataAccess.Interfaces;

public interface ISightingRepository
{
    Task<SightingsPage> GetSightingsAsync(SightingsParameters parameters, CancellationToken cancellationToken = default);
    Task<SightingsPage> GetSightingsByUserAsync(UserSightingsParameters parameters, CancellationToken cancellationToken = default);
    Task<SightingsPage> GetReviewQueueAsync(ReviewQueueParameters parameters, CancellationToken cancellationToken = default);
    Task<SightingListItem?> GetSightingAsync(int id, CancellationToken cancellationToken = default);
    Task<SightingDetailDto?> GetSightingDetailAsync(int sightingId, CancellationToken cancellationToken = default);
    Task<SightingPhotoResult?> GetSightingPhotoAsync(int sightingId, CancellationToken cancellationToken = default);
    Task<CreateSightingResponse> CreateSightingAsync(CreateSightingRequest request, int userId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> ReviewSightingAsync(int sightingId, ReviewSightingRequest request, int reviewerUserId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateSightingPhotoAsync(int sightingId, int userId, byte[] photoData, string contentType, CancellationToken cancellationToken = default);
}
