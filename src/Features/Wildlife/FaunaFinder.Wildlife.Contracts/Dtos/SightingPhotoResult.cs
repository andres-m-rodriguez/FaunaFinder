namespace FaunaFinder.Wildlife.Contracts.Dtos;

public sealed record SightingPhotoResult(
    byte[] PhotoData,
    string ContentType
);
