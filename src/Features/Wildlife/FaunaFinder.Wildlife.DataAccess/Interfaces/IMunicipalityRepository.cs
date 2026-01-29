using FaunaFinder.Pagination.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;

namespace FaunaFinder.Wildlife.DataAccess.Interfaces;

public interface IMunicipalityRepository
{
    Task<IReadOnlyList<MunicipalityForListDto>> GetAllMunicipalitiesAsync(
        CancellationToken cancellationToken = default);

    Task<MunicipalityForDetailDto?> GetMunicipalityDetailAsync(
        int municipalityId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MunicipalityCardDto>> GetMunicipalitiesWithSpeciesCountAsync(
        MunicipalityParameters parameters,
        CancellationToken cancellationToken = default);

    Task<int> GetTotalMunicipalitiesCountAsync(
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<CursorPage<MunicipalityCardDto>> GetMunicipalitiesCursorPageAsync(
        CursorPageRequest request,
        CancellationToken cancellationToken = default);
}
