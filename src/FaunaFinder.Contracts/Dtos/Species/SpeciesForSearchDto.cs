using FaunaFinder.Contracts.Localization;

namespace FaunaFinder.Contracts.Dtos.Species;

public sealed record SpeciesForSearchDto(
    int Id,
    List<LocaleValue> CommonName,
    string ScientificName,
    IReadOnlyList<string> MunicipalityNames
);
