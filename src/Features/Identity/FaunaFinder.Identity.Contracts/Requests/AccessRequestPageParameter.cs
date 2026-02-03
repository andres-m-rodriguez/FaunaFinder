namespace FaunaFinder.Identity.Contracts.Requests;

public record AccessRequestPageParameter(
    string? Cursor,
    int PageSize = 20,
    string? Search = null,
    string? Status = null
);
