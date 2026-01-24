using FaunaFinder.Contracts.Dtos.FwsLinks;
using FaunaFinder.Contracts.Localization;

namespace FaunaFinder.Contracts.Dtos.Species;

public sealed record SpeciesForDetailDto(
    int Id,
    List<LocaleValue> CommonName,
    string ScientificName,
    List<FwsLinkDto> FwsLinks,
    List<SpeciesMunicipalityDto> Municipalities,
    List<SpeciesLocationDto> Locations
);

public sealed record SpeciesMunicipalityDto(
    int Id,
    string Name
);
