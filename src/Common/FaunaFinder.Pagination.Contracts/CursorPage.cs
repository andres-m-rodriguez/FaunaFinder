namespace FaunaFinder.Pagination.Contracts;

public record CursorPage<T>(
    IReadOnlyList<T> Items,
    string? NextCursor,
    bool HasMore
);
