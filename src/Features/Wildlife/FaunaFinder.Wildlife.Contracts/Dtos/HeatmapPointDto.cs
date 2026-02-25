namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record HeatmapPointDto(
    double Latitude,
    double Longitude,
    double Intensity,
    bool IsFauna
);
