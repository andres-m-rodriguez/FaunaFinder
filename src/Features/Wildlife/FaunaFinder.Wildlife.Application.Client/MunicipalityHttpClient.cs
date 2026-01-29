using System.Net.Http.Json;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Pagination.Contracts;

namespace FaunaFinder.Wildlife.Application.Client;

public sealed class MunicipalityHttpClient : IMunicipalityHttpClient
{
    private readonly HttpClient _httpClient;

    public MunicipalityHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<MunicipalityForListDto>> GetAllMunicipalitiesAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<IReadOnlyList<MunicipalityForListDto>>(
            "api/municipalities",
            cancellationToken);
        return result ?? [];
    }

    public async Task<MunicipalityForDetailDto?> GetMunicipalityDetailAsync(
        int municipalityId,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<MunicipalityForDetailDto>(
            $"api/municipalities/{municipalityId}",
            cancellationToken);
    }

    public async Task<IReadOnlyList<MunicipalityCardDto>> GetMunicipalitiesWithSpeciesCountAsync(
        MunicipalityParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var queryString = BuildMunicipalityQueryString(parameters);
        var result = await _httpClient.GetFromJsonAsync<IReadOnlyList<MunicipalityCardDto>>(
            $"api/municipalities/cards{queryString}",
            cancellationToken);
        return result ?? [];
    }

    public async Task<int> GetTotalMunicipalitiesCountAsync(
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var queryString = string.IsNullOrEmpty(search) ? "" : $"?search={Uri.EscapeDataString(search)}";
        return await _httpClient.GetFromJsonAsync<int>(
            $"api/municipalities/count{queryString}",
            cancellationToken);
    }

    private static string BuildMunicipalityQueryString(MunicipalityParameters parameters)
    {
        var queryParams = new List<string>
        {
            $"pageSize={parameters.PageSize}",
            $"page={parameters.Page}"
        };

        if (!string.IsNullOrEmpty(parameters.Search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(parameters.Search)}");
        }

        return "?" + string.Join("&", queryParams);
    }

    public async Task<CursorPage<MunicipalityCardDto>> GetMunicipalitiesCursorPageAsync(
        CursorPageRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryString = BuildCursorQueryString(request);
        var result = await _httpClient.GetFromJsonAsync<CursorPage<MunicipalityCardDto>>(
            $"api/municipalities/cursor{queryString}",
            cancellationToken);
        return result ?? new CursorPage<MunicipalityCardDto>([], null, false);
    }

    private static string BuildCursorQueryString(CursorPageRequest request)
    {
        var queryParams = new List<string>
        {
            $"pageSize={request.PageSize}"
        };

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
