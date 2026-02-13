using System.Net.Http.Json;
using FaunaFinder.Pagination.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;

namespace FaunaFinder.Wildlife.Application.Client;

public sealed class SpeciesHttpClient(HttpClient httpClient) : ISpeciesHttpClient
{
    public async Task<IReadOnlyList<SpeciesForListDto>> GetSpeciesByMunicipalityAsync(
        int municipalityId,
        CancellationToken cancellationToken = default
    )
    {
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<SpeciesForListDto>>(
            $"api/species/by-municipality/{municipalityId}",
            cancellationToken
        );
        return result ?? [];
    }

    public async Task<SpeciesForDetailDto?> GetSpeciesDetailAsync(
        int speciesId,
        CancellationToken cancellationToken = default
    )
    {
        return await httpClient.GetFromJsonAsync<SpeciesForDetailDto>(
            $"api/species/{speciesId}",
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<SpeciesForSearchDto>> GetSpeciesAsync(
        SpeciesParameters parameters,
        CancellationToken cancellationToken = default
    )
    {
        var queryString = BuildSpeciesQueryString(parameters);
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<SpeciesForSearchDto>>(
            $"api/species{queryString}",
            cancellationToken
        );
        return result ?? [];
    }

    public async Task<int> GetTotalSpeciesCountAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        var queryString = string.IsNullOrEmpty(search) ? "" : $"?search={Uri.EscapeDataString(search)}";
        return await httpClient.GetFromJsonAsync<int>(
            $"api/species/count{queryString}",
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<SpeciesNearbyDto>> GetSpeciesNearbyAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        CancellationToken cancellationToken = default
    )
    {
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<SpeciesNearbyDto>>(
            $"api/species/nearby?latitude={latitude}&longitude={longitude}&radiusMeters={radiusMeters}",
            cancellationToken
        );
        return result ?? [];
    }

    private static string BuildSpeciesQueryString(SpeciesParameters parameters)
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

        if (parameters.MunicipalityId.HasValue)
        {
            queryParams.Add($"municipalityId={parameters.MunicipalityId.Value}");
        }

        return "?" + string.Join("&", queryParams);
    }

    public async Task<CursorPage<SpeciesForSearchDto>> GetSpeciesCursorPageAsync(
        CursorPageParameter request,
        CancellationToken cancellationToken = default
    )
    {
        var queryString = BuildCursorQueryString(request);
        var result = await httpClient.GetFromJsonAsync<CursorPage<SpeciesForSearchDto>>(
            $"api/species/cursor{queryString}",
            cancellationToken
        );
        return result ?? new CursorPage<SpeciesForSearchDto>([], null, false);
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
