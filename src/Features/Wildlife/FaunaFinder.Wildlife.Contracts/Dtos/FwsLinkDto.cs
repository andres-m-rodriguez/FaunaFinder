namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record FwsLinkDto(
    int Id,
    NrcsPracticeDto NrcsPractice,
    FwsActionDto FwsAction,
    string? Justification
);
