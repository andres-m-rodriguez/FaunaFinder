using System.Net.Http.Json;
using FaunaFinder.Wildlife.Contracts.Dtos;

namespace FaunaFinder.Wildlife.Application.Client;

public sealed class AnalyticsHttpClient(HttpClient httpClient) : IAnalyticsHttpClient
{
    public async Task<IReadOnlyList<MunicipalitySpeciesCountDto>> GetSpeciesPerMunicipalityAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync("/api/analytics/species-per-municipality", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }
            var result = await response.Content.ReadFromJsonAsync<List<MunicipalitySpeciesCountDto>>(cancellationToken);
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<SpeciesMunicipalityCountDto>> GetMunicipalitiesPerSpeciesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync("/api/analytics/municipalities-per-species", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }
            var result = await response.Content.ReadFromJsonAsync<List<SpeciesMunicipalityCountDto>>(cancellationToken);
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<SpeciesSightingCountDto>> GetTopSightedSpeciesAsync(
        int limit = 15,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/analytics/top-sighted-species?limit={limit}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }
            var result = await response.Content.ReadFromJsonAsync<List<SpeciesSightingCountDto>>(cancellationToken);
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }
}
