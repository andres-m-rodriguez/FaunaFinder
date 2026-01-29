using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record SpeciesForSearchDto(
    int Id,
    List<LocaleValue> CommonName,
    string ScientificName,
    IReadOnlyList<string> MunicipalityNames
);
