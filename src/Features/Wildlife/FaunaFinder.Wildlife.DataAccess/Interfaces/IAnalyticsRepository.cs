using FaunaFinder.Wildlife.Contracts.Dtos;

namespace FaunaFinder.Wildlife.DataAccess.Interfaces;

public interface IAnalyticsRepository
{
    Task<IReadOnlyList<MunicipalitySpeciesCountDto>> GetSpeciesPerMunicipalityAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpeciesMunicipalityCountDto>> GetMunicipalitiesPerSpeciesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpeciesSightingCountDto>> GetTopSightedSpeciesAsync(
        int limit,
        CancellationToken cancellationToken = default);
}
