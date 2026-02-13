using System.Net.Http.Json;
using FaunaFinder.Wildlife.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Requests;
using FaunaFinder.Wildlife.Contracts.Responses;

namespace FaunaFinder.Wildlife.Application.Client;

public sealed class WildlifeHttpClient(HttpClient httpClient) : IWildlifeHttpClient
{
    public async Task<IReadOnlyList<SpeciesForSearchDto>> SearchSpeciesAsync(
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return [];

        var url = $"/api/species?search={Uri.EscapeDataString(query)}&pageSize={limit}&page=1";
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<SpeciesForSearchDto>>(
            url,
            cancellationToken
        );
        return result ?? [];
    }

    public async Task<SightingsPage> GetMySightingsAsync(
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/sightings/mine?page={page}&pageSize={pageSize}";
        var result = await httpClient.GetFromJsonAsync<SightingsPage>(url, cancellationToken);
        return result ?? new SightingsPage([], 0, page, pageSize);
    }

    public async Task<SightingsPage> GetSightingsAsync(
        int page,
        int pageSize,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/sightings?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(status))
        {
            url += $"&status={Uri.EscapeDataString(status)}";
        }

        var result = await httpClient.GetFromJsonAsync<SightingsPage>(url, cancellationToken);
        return result ?? new SightingsPage([], 0, page, pageSize);
    }

    public async Task<CreateSightingResponse> CreateSightingAsync(
        CreateSightingRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/sightings", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<SightingCreatedResponse>(cancellationToken);
            return new CreateSightingResponse(result?.Id, null, true);
        }

        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        return new CreateSightingResponse(null, error, false);
    }

    public async Task<SightingDetailDto?> GetSightingDetailAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<SightingDetailDto>(
            $"/api/sightings/{id}",
            cancellationToken
        );
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

        var response = await httpClient.PatchAsync(
            $"/api/sightings/{sightingId}/photo",
            content,
            cancellationToken
        );

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReviewSightingAsync(
        int sightingId,
        string status,
        string? reviewNotes,
        CancellationToken cancellationToken = default)
    {
        var request = new ReviewSightingRequest(status, reviewNotes);
        var response = await httpClient.PostAsJsonAsync(
            $"/api/sightings/{sightingId}/review",
            request,
            cancellationToken
        );

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
}
