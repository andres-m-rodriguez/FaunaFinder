namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record SpeciesLocationDto(
    int Id,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    string? Description
);
