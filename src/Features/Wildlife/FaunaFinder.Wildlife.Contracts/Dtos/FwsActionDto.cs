using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record FwsActionDto(int Id, string Code, List<LocaleValue> Name);
