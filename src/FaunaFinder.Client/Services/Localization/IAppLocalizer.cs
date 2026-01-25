using FaunaFinder.Contracts.Localization;

namespace FaunaFinder.Client.Services.Localization;

public interface IAppLocalizer
{
    string this[string key] { get; }
    string this[string key, params object[] args] { get; }
    string CurrentLanguage { get; }
    bool IsSpanish { get; }
    void SetLanguage(string languageCode);
    event Action? OnLanguageChanged;

    /// <summary>
    /// Gets the localized value based on the current language setting.
    /// Returns the translation if available, otherwise falls back to the default language.
    /// </summary>
    string GetLocalizedValue(IEnumerable<LocaleValue> values);
}
