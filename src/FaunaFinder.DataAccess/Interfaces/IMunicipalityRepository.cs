using FaunaFinder.Contracts.Dtos.Municipalities;

namespace FaunaFinder.DataAccess.Interfaces;

public interface IMunicipalityRepository
{
    Task<IReadOnlyList<MunicipalityForListDto>> GetAllMunicipalitiesAsync(
        CancellationToken cancellationToken = default);

    Task<MunicipalityForDetailDto?> GetMunicipalityDetailAsync(
        int municipalityId,
        CancellationToken cancellationToken = default);
}
