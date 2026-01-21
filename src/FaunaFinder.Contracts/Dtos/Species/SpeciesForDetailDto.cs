using FaunaFinder.Contracts.Dtos.FwsLinks;

namespace FaunaFinder.Contracts.Dtos.Species;

public sealed record SpeciesForDetailDto(
    int Id,
    string CommonName,
    string ScientificName,
    List<FwsLinkDto> FwsLinks
);
