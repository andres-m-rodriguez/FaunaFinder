namespace FaunaFinder.Wildlife.Contracts;

public record SightingsPage(
    List<SightingListItem> Items,
    int TotalCount,
    int Page,
    int PageSize);
