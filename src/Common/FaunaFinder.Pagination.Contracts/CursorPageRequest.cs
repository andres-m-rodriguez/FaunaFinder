namespace FaunaFinder.Pagination.Contracts;

public record CursorPageRequest(
    string? Cursor,
    int PageSize = 20,
    string? Search = null
);
