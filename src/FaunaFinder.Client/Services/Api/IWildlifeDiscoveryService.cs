namespace FaunaFinder.Client.Services.Api;

public interface IWildlifeDiscoveryService
{
    Task<IReadOnlyList<SpeciesSearchResult>> SearchSpeciesAsync(
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default);

    Task<SightingsPage> GetMySightingsAsync(
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default);

    Task<CreateSightingResult> CreateSightingAsync(
        CreateSightingRequest request,
        CancellationToken cancellationToken = default);
}

public record SpeciesSearchResult(int Id, string CommonName, string ScientificName);

public record SightingsPage(
    List<SightingListItem> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record SightingListItem(
    int Id,
    int SpeciesId,
    string? SpeciesName,
    string Mode,
    string Confidence,
    string Count,
    string Status,
    DateTime ObservedAt,
    DateTime CreatedAt,
    double Latitude,
    double Longitude,
    bool HasPhoto);

public record CreateSightingRequest(
    int SpeciesId,
    double Latitude,
    double Longitude,
    DateTime ObservedAt,
    string Mode,
    string Confidence,
    string Count,
    int Behaviors,
    int Evidence,
    string? Weather,
    string? Notes,
    byte[]? PhotoData,
    string? PhotoContentType);

public record CreateSightingResult(int? Id, string? Error, bool Success);
