using FaunaFinder.Wildlife.Contracts.Dtos;

namespace FaunaFinder.Wildlife.Application.Client;

public interface IAnalyticsHttpClient
{
    Task<IReadOnlyList<MunicipalitySpeciesCountDto>> GetSpeciesPerMunicipalityAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpeciesMunicipalityCountDto>> GetMunicipalitiesPerSpeciesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpeciesSightingCountDto>> GetTopSightedSpeciesAsync(
        int limit = 15,
        CancellationToken cancellationToken = default);
}
