using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record NrcsPracticeDto(int Id, string Code, List<LocaleValue> Name);
