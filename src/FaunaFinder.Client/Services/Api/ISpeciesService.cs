using FaunaFinder.Contracts.Dtos.Species;
using FaunaFinder.Contracts.Parameters;
using FaunaFinder.Pagination.Contracts;

namespace FaunaFinder.Client.Services.Api;

public interface ISpeciesService
{
    Task<IReadOnlyList<SpeciesForListDto>> GetSpeciesByMunicipalityAsync(
        int municipalityId,
        CancellationToken cancellationToken = default);

    Task<SpeciesForDetailDto?> GetSpeciesDetailAsync(
        int speciesId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpeciesForSearchDto>> GetSpeciesAsync(
        SpeciesParameters parameters,
        CancellationToken cancellationToken = default);

    Task<int> GetTotalSpeciesCountAsync(
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpeciesNearbyDto>> GetSpeciesNearbyAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        CancellationToken cancellationToken = default);

    Task<CursorPage<SpeciesForSearchDto>> GetSpeciesCursorPageAsync(
        CursorPageRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpeciesLocationsDto>> GetSpeciesLocationsBatchAsync(
        IEnumerable<int> speciesIds,
        CancellationToken cancellationToken = default);
}
