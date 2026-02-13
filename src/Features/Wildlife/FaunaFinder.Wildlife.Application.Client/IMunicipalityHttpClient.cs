using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;

namespace FaunaFinder.Wildlife.Application.Client;

public interface IMunicipalityHttpClient
{
    Task<IReadOnlyList<MunicipalityForListDto>> GetAllMunicipalitiesAsync(
        CancellationToken cancellationToken = default
    );

    Task<MunicipalityForDetailDto?> GetMunicipalityDetailAsync(
        int municipalityId,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<MunicipalityCardDto> GetMunicipalityCardsAsync(
        MunicipalityParameters parameters,
        CancellationToken cancellationToken = default
    );

    Task<int> GetTotalMunicipalitiesCountAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    );
}
