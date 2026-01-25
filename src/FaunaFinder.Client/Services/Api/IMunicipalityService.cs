using FaunaFinder.Contracts.Dtos.Municipalities;
using FaunaFinder.Contracts.Parameters;
using FaunaFinder.Pagination.Contracts;

namespace FaunaFinder.Client.Services.Api;

public interface IMunicipalityService
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
