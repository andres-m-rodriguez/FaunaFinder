namespace FaunaFinder.WildlifeDiscovery.Contracts;

public record SightingsPage(
    List<SightingListItem> Items,
    int TotalCount,
    int Page,
    int PageSize);
