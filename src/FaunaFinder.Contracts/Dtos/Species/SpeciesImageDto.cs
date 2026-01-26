namespace FaunaFinder.Contracts.Dtos.Species;

public sealed record SpeciesImageDto(
    int Id,
    int SpeciesId,
    string ContentType,
    string? FileName,
    string? Description,
    bool IsPrimary,
    DateTime CreatedAt
);

public sealed record SpeciesImageDataDto(
    int Id,
    int SpeciesId,
    byte[] ImageData,
    string ContentType,
    string? FileName,
    string? Description,
    bool IsPrimary,
    DateTime CreatedAt
);
