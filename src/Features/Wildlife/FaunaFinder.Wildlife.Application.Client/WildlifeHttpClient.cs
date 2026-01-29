using System.Net.Http.Json;
using FaunaFinder.Wildlife.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Requests;

namespace FaunaFinder.Wildlife.Application.Client;

public sealed class WildlifeHttpClient : IWildlifeHttpClient
{
    private readonly HttpClient _httpClient;

    public WildlifeHttpClient(HttpClient httpClient)
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

        var url = $"/api/wildlife/species/search?query={Uri.EscapeDataString(query)}&limit={limit}";
        var result = await _httpClient.GetFromJsonAsync<List<SpeciesSearchResult>>(url, cancellationToken);
        return result ?? [];
    }

    public async Task<SightingsPage> GetMySightingsAsync(
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/wildlife/my-sightings?page={page}&pageSize={pageSize}";
        var result = await _httpClient.GetFromJsonAsync<SightingsPage>(url, cancellationToken);
        return result ?? new SightingsPage([], 0, page, pageSize);
    }

    public async Task<SightingsPage> GetSightingsAsync(
        int page,
        int pageSize,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/wildlife/sightings?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(status))
        {
            url += $"&status={Uri.EscapeDataString(status)}";
        }

        var result = await _httpClient.GetFromJsonAsync<SightingsPage>(url, cancellationToken);
        return result ?? new SightingsPage([], 0, page, pageSize);
    }

    public async Task<SightingsPage> GetReviewQueueAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/wildlife/review-queue?page={page}&pageSize={pageSize}";
        var result = await _httpClient.GetFromJsonAsync<SightingsPage>(url, cancellationToken);
        return result ?? new SightingsPage([], 0, page, pageSize);
    }

    public async Task<CreateSightingResponse> CreateSightingAsync(
        CreateSightingRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/wildlife/sightings", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<IdResponse>(cancellationToken);
            return new CreateSightingResponse(result?.Id, null, true);
        }

        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        return new CreateSightingResponse(null, error, false);
    }

    public async Task<SightingDetailDto?> GetSightingDetailAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/wildlife/sightings/{id}";
        return await _httpClient.GetFromJsonAsync<SightingDetailDto>(url, cancellationToken);
    }

    public async Task<bool> UploadSightingPhotoAsync(
        int sightingId,
        byte[] photoData,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(photoData);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "photo", "photo" + GetFileExtension(contentType));

        var url = $"/api/wildlife/sightings/{sightingId}/photo";
        var response = await _httpClient.PatchAsync(url, content, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReviewSightingAsync(
        int sightingId,
        string status,
        string? reviewNotes,
        CancellationToken cancellationToken = default)
    {
        var request = new ReviewSightingRequest(status, reviewNotes);
        var url = $"/api/wildlife/sightings/{sightingId}/review";
        var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    private static string GetFileExtension(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/gif" => ".gif",
        "image/webp" => ".webp",
        _ => ".jpg"
    };

    private record IdResponse(int Id);
}
