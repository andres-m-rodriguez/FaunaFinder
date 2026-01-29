namespace FaunaFinder.Wildlife.Contracts.Parameters;

public sealed record UserSightingsParameters(
    int UserId,
    int Page = 1,
    int PageSize = 20
);
