using FaunaFinder.Contracts.Dtos.Species;

namespace FaunaFinder.Api.Services.Localization;

public interface IAppLocalizer
{
    string this[string key] { get; }
    string this[string key, params object[] args] { get; }
    string CurrentLanguage { get; }
    bool IsSpanish { get; }
    void SetLanguage(string languageCode);
    event Action? OnLanguageChanged;

    /// <summary>
    /// Gets the translated species name based on the current language setting.
    /// Returns the translation if available, otherwise falls back to the English name.
    /// </summary>
    string GetSpeciesName(string englishName, IEnumerable<SpeciesTranslationDto>? translations);
}
