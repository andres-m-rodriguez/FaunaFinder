using System.Net.Http.Json;
using FaunaFinder.Pagination.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;

namespace FaunaFinder.Wildlife.Application.Client;

public sealed class MunicipalityHttpClient(HttpClient httpClient) : IMunicipalityHttpClient
{
    public async Task<IReadOnlyList<MunicipalityForListDto>> GetAllMunicipalitiesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<MunicipalityForListDto>>(
            "api/municipalities",
            cancellationToken
        );
        return result ?? [];
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

    public async Task<IReadOnlyList<MunicipalityCardDto>> GetMunicipalitiesWithSpeciesCountAsync(
        MunicipalityParameters parameters,
        CancellationToken cancellationToken = default
    )
    {
        var queryString = BuildMunicipalityQueryString(parameters);
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<MunicipalityCardDto>>(
            $"api/municipalities/cards{queryString}",
            cancellationToken
        );
        return result ?? [];
    }

    public async Task<int> GetTotalMunicipalitiesCountAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        var queryString = string.IsNullOrEmpty(search)
            ? ""
            : $"?search={Uri.EscapeDataString(search)}";
        return await httpClient.GetFromJsonAsync<int>(
            $"api/municipalities/count{queryString}",
            cancellationToken
        );
    }

    private static string BuildMunicipalityQueryString(MunicipalityParameters parameters)
    {
        var queryParams = new List<string>
        {
            $"pageSize={parameters.PageSize}",
            $"page={parameters.Page}",
        };

        if (!string.IsNullOrEmpty(parameters.Search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(parameters.Search)}");
        }

        return "?" + string.Join("&", queryParams);
    }

    public async Task<CursorPage<MunicipalityCardDto>> GetMunicipalitiesCursorPageAsync(
        CursorPageParameter request,
        CancellationToken cancellationToken = default
    )
    {
        var queryString = BuildCursorQueryString(request);
        var result = await httpClient.GetFromJsonAsync<CursorPage<MunicipalityCardDto>>(
            $"api/municipalities/cursor{queryString}",
            cancellationToken
        );
        return result ?? new CursorPage<MunicipalityCardDto>([], null, false);
    }

    private static string BuildCursorQueryString(CursorPageParameter request)
    {
        var queryParams = new List<string> { $"pageSize={request.PageSize}" };

        if (!string.IsNullOrEmpty(request.Cursor))
        {
            queryParams.Add($"cursor={Uri.EscapeDataString(request.Cursor)}");
        }

        if (!string.IsNullOrEmpty(request.Search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(request.Search)}");
        }

        return "?" + string.Join("&", queryParams);
    }
}
