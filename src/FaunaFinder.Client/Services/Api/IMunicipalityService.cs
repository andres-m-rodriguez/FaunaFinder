using FaunaFinder.Contracts.Dtos.Municipalities;
using FaunaFinder.Contracts.Parameters;

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
}
