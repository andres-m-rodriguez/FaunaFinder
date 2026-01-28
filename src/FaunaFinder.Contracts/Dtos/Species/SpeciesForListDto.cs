using FaunaFinder.Contracts.Dtos.FwsLinks;
using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Contracts.Dtos.Species;

public sealed record SpeciesForListDto(
    int Id,
    List<LocaleValue> CommonName,
    string ScientificName,
    List<FwsLinkDto> FwsLinks
);
