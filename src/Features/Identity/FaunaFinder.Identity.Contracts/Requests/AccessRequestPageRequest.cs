namespace FaunaFinder.Identity.Contracts.Requests;

public record AccessRequestPageRequest(
    string? Cursor,
    int PageSize = 20,
    string? Search = null,
    string? Status = null
);
