using FaunaFinder.Contracts.Dtos.FwsLinks;

namespace FaunaFinder.Contracts.Dtos.Species;

public sealed record SpeciesForListDto(
    int Id,
    string CommonName,
    string ScientificName,
    List<FwsLinkDto> FwsLinks
);
