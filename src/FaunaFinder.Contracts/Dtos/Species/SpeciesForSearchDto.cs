using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Contracts.Dtos.Species;

public sealed record SpeciesForSearchDto(
    int Id,
    List<LocaleValue> CommonName,
    string ScientificName,
    IReadOnlyList<string> MunicipalityNames
);
