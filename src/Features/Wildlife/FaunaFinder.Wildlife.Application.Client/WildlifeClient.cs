using System.Net.Http.Json;
using FaunaFinder.Wildlife.Contracts;

namespace FaunaFinder.Wildlife.Application.Client;

public sealed class WildlifeClient(HttpClient httpClient) : IWildlifeClient
{
    public async Task<IReadOnlyList<SpeciesSearchResult>> SearchSpeciesAsync(string? query, int limit, CancellationToken cancellationToken = default)
    {
        var url = $"/api/wildlife/species/search?limit={limit}";
        if (!string.IsNullOrWhiteSpace(query))
        {
            url += $"&query={Uri.EscapeDataString(query)}";
        }

        var result = await httpClient.GetFromJsonAsync<List<SpeciesSearchResult>>(url, cancellationToken);
        return result ?? [];
    }

    public async Task<CreateSightingResponse> CreateSightingAsync(CreateSightingRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/wildlife/sightings", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CreateSightingIdResponse>(cancellationToken);
            return new CreateSightingResponse(result?.Id, null, true);
        }

        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        return new CreateSightingResponse(null, error, false);
    }

    public async Task<SightingsPage> GetSightingsAsync(int page, int pageSize, string? status = null, CancellationToken cancellationToken = default)
    {
        var url = $"/api/wildlife/sightings?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(status))
        {
            url += $"&status={Uri.EscapeDataString(status)}";
        }

        var result = await httpClient.GetFromJsonAsync<SightingsPage>(url, cancellationToken);
        return result ?? new SightingsPage([], 0, page, pageSize);
    }

    public async Task<SightingsPage> GetMySightingsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var url = $"/api/wildlife/my-sightings?page={page}&pageSize={pageSize}";
        var result = await httpClient.GetFromJsonAsync<SightingsPage>(url, cancellationToken);
        return result ?? new SightingsPage([], 0, page, pageSize);
    }

    public async Task<SightingsPage> GetReviewQueueAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var url = $"/api/wildlife/review-queue?page={page}&pageSize={pageSize}";
        var result = await httpClient.GetFromJsonAsync<SightingsPage>(url, cancellationToken);
        return result ?? new SightingsPage([], 0, page, pageSize);
    }

    private record CreateSightingIdResponse(int Id);
}
