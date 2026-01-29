using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record SpeciesForDetailDto(
    int Id,
    List<LocaleValue> CommonName,
    string ScientificName,
    List<FwsLinkDto> FwsLinks,
    List<SpeciesMunicipalityDto> Municipalities,
    List<SpeciesLocationDto> Locations,
    bool HasProfileImage
);
