namespace FaunaFinder.Pagination.Contracts;

public record CursorPageParameter(
    string? Cursor,
    int PageSize = 20,
    string? Search = null
);
