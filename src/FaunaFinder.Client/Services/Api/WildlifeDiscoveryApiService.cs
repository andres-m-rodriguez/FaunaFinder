using System.Net.Http.Json;

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

    public async Task<CreateSightingResult> CreateSightingAsync(
        CreateSightingRequest request,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(request.SpeciesId.ToString()), "speciesId");
        content.Add(new StringContent(request.Latitude.ToString()), "latitude");
        content.Add(new StringContent(request.Longitude.ToString()), "longitude");
        content.Add(new StringContent(request.ObservedAt.ToString("O")), "observedAt");
        content.Add(new StringContent(request.Mode), "mode");
        content.Add(new StringContent(request.Confidence), "confidence");
        content.Add(new StringContent(request.Count), "count");
        content.Add(new StringContent(request.Behaviors.ToString()), "behaviors");
        content.Add(new StringContent(request.Evidence.ToString()), "evidence");

        if (!string.IsNullOrWhiteSpace(request.Weather))
            content.Add(new StringContent(request.Weather), "weather");

        if (!string.IsNullOrWhiteSpace(request.Notes))
            content.Add(new StringContent(request.Notes), "notes");

        if (request.PhotoData is not null && request.PhotoContentType is not null)
        {
            var photoContent = new ByteArrayContent(request.PhotoData);
            photoContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.PhotoContentType);
            content.Add(photoContent, "photo", "photo.jpg");
        }

        var response = await _httpClient.PostAsync("api/wildlife/sightings", content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CreateSightingResponse>(cancellationToken: cancellationToken);
            return new CreateSightingResult(result?.Id, null, true);
        }

        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        return new CreateSightingResult(null, error, false);
    }

    private record CreateSightingResponse(int Id);
}
