using FaunaFinder.Contracts.Dtos.Species;
using FaunaFinder.Contracts.Parameters;

namespace FaunaFinder.DataAccess.Interfaces;

public interface ISpeciesRepository
{
    Task<IReadOnlyList<SpeciesForListDto>> GetSpeciesByMunicipalityAsync(
        int municipalityId,
        CancellationToken cancellationToken = default);

    Task<SpeciesForDetailDto?> GetSpeciesDetailAsync(
        int speciesId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpeciesForSearchDto>> SearchSpeciesAsync(
        SpeciesParameters parameters,
        CancellationToken cancellationToken = default);
}
