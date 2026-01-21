using FaunaFinder.Contracts.Dtos.FwsActions;
using FaunaFinder.Contracts.Dtos.NrcsPractices;

namespace FaunaFinder.Contracts.Dtos.FwsLinks;

public sealed record FwsLinkDto(
    int Id,
    NrcsPracticeDto NrcsPractice,
    FwsActionDto FwsAction,
    string? Justification
);
