using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record SpeciesCategoryDto(int Id, string Code, IReadOnlyList<LocaleValue> Name);
