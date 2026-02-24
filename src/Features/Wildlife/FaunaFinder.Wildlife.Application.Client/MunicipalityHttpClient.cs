using System.Net.Http.Json;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;

namespace FaunaFinder.Wildlife.Application.Client;

public sealed class MunicipalityHttpClient(HttpClient httpClient) : IMunicipalityHttpClient
{
    private IReadOnlyList<MunicipalityForListDto>? _cachedMunicipalities;

    public async Task<IReadOnlyList<MunicipalityForListDto>> GetAllMunicipalitiesAsync(
        CancellationToken cancellationToken = default
    )
    {
        if (_cachedMunicipalities is not null)
        {
            return _cachedMunicipalities;
        }

        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<MunicipalityForListDto>>(
            "api/municipalities",
            cancellationToken
        );
        _cachedMunicipalities = result ?? [];
        return _cachedMunicipalities;
    }

    public async Task<MunicipalityForDetailDto?> GetMunicipalityDetailAsync(
        int municipalityId,
        CancellationToken cancellationToken = default
    )
    {
        return await httpClient.GetFromJsonAsync<MunicipalityForDetailDto>(
            $"api/municipalities/{municipalityId}",
            cancellationToken
        );
    }

    public IAsyncEnumerable<MunicipalityCardDto> GetMunicipalityCardsAsync(
        MunicipalityParameters parameters,
        CancellationToken cancellationToken = default
    )
    {
        var queryString = BuildMunicipalityQueryString(parameters);
        return httpClient.GetFromJsonAsAsyncEnumerable<MunicipalityCardDto>(
            $"api/municipalities/cards{queryString}",
            cancellationToken
        )!;
    }

    public async Task<int> GetTotalMunicipalitiesCountAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        var queryString = string.IsNullOrEmpty(search) ? "" : $"?search={Uri.EscapeDataString(search)}";
        return await httpClient.GetFromJsonAsync<int>(
            $"api/municipalities/count{queryString}",
            cancellationToken
        );
    }

    private static string BuildMunicipalityQueryString(MunicipalityParameters parameters)
    {
        var queryParams = new List<string> { $"pageSize={parameters.PageSize}" };

        if (parameters.Cursor.HasValue)
        {
            queryParams.Add($"cursor={parameters.Cursor.Value}");
        }

        if (!string.IsNullOrEmpty(parameters.Search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(parameters.Search)}");
        }

        return "?" + string.Join("&", queryParams);
    }
}
