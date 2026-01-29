namespace FaunaFinder.Wildlife.Contracts.Dtos;

/// <summary>
/// DTO for species profile image data retrieval.
/// </summary>
public sealed record SpeciesProfileImageDto(
    int SpeciesId,
    byte[] ImageData,
    string ContentType
);
