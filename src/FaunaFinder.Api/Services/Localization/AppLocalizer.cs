using FaunaFinder.Contracts.Dtos.Species;

namespace FaunaFinder.Api.Services.Localization;

public sealed class AppLocalizer : IAppLocalizer
{
    private IReadOnlyDictionary<string, string> _currentTranslations = Translations.English;

    public string CurrentLanguage { get; private set; } = "en-US";

    public bool IsSpanish => CurrentLanguage.StartsWith("es");

    public event Action? OnLanguageChanged;

    public string this[string key] =>
        _currentTranslations.TryGetValue(key, out var value) ? value : key;

    public string this[string key, params object[] args] =>
        string.Format(this[key], args);

    public void SetLanguage(string languageCode)
    {
        if (languageCode == CurrentLanguage) return;

        CurrentLanguage = languageCode;
        _currentTranslations = languageCode.StartsWith("es")
            ? Translations.Spanish
            : Translations.English;

        OnLanguageChanged?.Invoke();
    }

    public string GetSpeciesName(string englishName, IEnumerable<SpeciesTranslationDto>? translations)
    {
        if (!IsSpanish || translations is null)
            return englishName;

        var spanishTranslation = translations.FirstOrDefault(t => t.LanguageCode.StartsWith("es"));
        return spanishTranslation?.CommonName ?? englishName;
    }
}
