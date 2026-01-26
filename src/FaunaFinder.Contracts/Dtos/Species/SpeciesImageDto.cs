namespace FaunaFinder.Contracts.Dtos.Species;

/// <summary>
/// DTO for species profile image data retrieval.
/// </summary>
public sealed record SpeciesProfileImageDto(
    int SpeciesId,
    byte[] ImageData,
    string ContentType
);
