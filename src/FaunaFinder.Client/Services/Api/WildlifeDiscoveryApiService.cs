using System.Net.Http.Json;
using FaunaFinder.WildlifeDiscovery.Contracts;

namespace FaunaFinder.Client.Services.Api;

public sealed class WildlifeDiscoveryApiService : IWildlifeDiscoveryService
{
    private readonly HttpClient _httpClient;

    public WildlifeDiscoveryApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<SpeciesSearchResult>> SearchSpeciesAsync(
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return [];

        var result = await _httpClient.GetFromJsonAsync<List<SpeciesSearchResult>>(
            $"api/wildlife/species/search?query={Uri.EscapeDataString(query)}&limit={limit}",
            cancellationToken);

        return result ?? [];
    }

    public async Task<SightingsPage> GetMySightingsAsync(
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<SightingsPage>(
            $"api/wildlife/my-sightings?page={page}&pageSize={pageSize}",
            cancellationToken);

        return result ?? new SightingsPage([], 0, page, pageSize);
    }

    public async Task<CreateSightingResponse> CreateSightingAsync(
        CreateSightingRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/wildlife/sightings", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<IdResponse>(cancellationToken: cancellationToken);
            return new CreateSightingResponse(result?.Id, null, true);
        }

        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        return new CreateSightingResponse(null, error, false);
    }

    private record IdResponse(int Id);
}
