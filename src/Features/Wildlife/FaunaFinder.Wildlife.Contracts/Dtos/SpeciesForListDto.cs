using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record SpeciesForListDto(
    int Id,
    List<LocaleValue> CommonName,
    string ScientificName,
    List<FwsLinkDto> FwsLinks
);
