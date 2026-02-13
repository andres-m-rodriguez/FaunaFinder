using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record FwsLinkDto(
    int Id,
    NrcsPracticeDto NrcsPractice,
    FwsActionDto FwsAction,
    List<LocaleValue> Justification
);
