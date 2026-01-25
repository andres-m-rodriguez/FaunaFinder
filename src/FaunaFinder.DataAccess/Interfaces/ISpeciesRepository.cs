using FaunaFinder.Contracts.Dtos.Species;
using FaunaFinder.Contracts.Parameters;
using FaunaFinder.Pagination.Contracts;

namespace FaunaFinder.DataAccess.Interfaces;

public interface ISpeciesRepository
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

    /// <summary>
    /// Gets species that have locations within the specified radius from the given coordinates.
    /// </summary>
    /// <param name="latitude">The latitude of the center point.</param>
    /// <param name="longitude">The longitude of the center point.</param>
    /// <param name="radiusMeters">The search radius in meters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of species found within the radius, ordered by distance.</returns>
    Task<IReadOnlyList<SpeciesNearbyDto>> GetSpeciesNearbyAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        CancellationToken cancellationToken = default);

    Task<CursorPage<SpeciesForSearchDto>> GetSpeciesCursorPageAsync(
        CursorPageRequest request,
        CancellationToken cancellationToken = default);
}
