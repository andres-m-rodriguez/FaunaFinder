namespace FaunaFinder.Contracts.Dtos.Species;

public sealed record SpeciesTranslationDto(
    string LanguageCode,
    string CommonName
);
