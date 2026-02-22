using System.Net.Http.Json;
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

    public IAsyncEnumerable<SpeciesForSearchDto> GetSpeciesAsync(
        SpeciesParameters parameters,
        CancellationToken cancellationToken = default
    )
    {
        var queryString = BuildSpeciesQueryString(parameters);
        return httpClient.GetFromJsonAsAsyncEnumerable<SpeciesForSearchDto>(
            $"api/species{queryString}",
            cancellationToken
        )!;
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

    public async Task<IReadOnlyList<SpeciesNearbyDto>> GetSpeciesInPolygonAsync(
        IReadOnlyList<PolygonCoordinate> coordinates,
        CancellationToken cancellationToken = default
    )
    {
        var parameters = new PolygonSearchParameters(coordinates);
        var response = await httpClient.PostAsJsonAsync(
            "api/species/in-polygon",
            parameters,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<SpeciesNearbyDto>>(
            cancellationToken
        );
        return result ?? [];
    }

    private static string BuildSpeciesQueryString(SpeciesParameters parameters)
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

        if (parameters.MunicipalityId.HasValue)
        {
            queryParams.Add($"municipalityId={parameters.MunicipalityId.Value}");
        }

        if (!string.IsNullOrEmpty(parameters.CategoryIds))
        {
            queryParams.Add($"categoryIds={Uri.EscapeDataString(parameters.CategoryIds)}");
        }

        return "?" + string.Join("&", queryParams);
    }

    public async Task<IReadOnlyList<SpeciesCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<SpeciesCategoryDto>>(
            "api/species/categories",
            cancellationToken
        );
        return result ?? [];
    }
}
