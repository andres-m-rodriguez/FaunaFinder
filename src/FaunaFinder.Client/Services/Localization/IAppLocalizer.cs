using BlazingSingularity.Signals;
using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Client.Services.Localization;

public interface IAppLocalizer
{
    string this[string key] { get; }
    string this[string key, params object[] args] { get; }
    string CurrentLanguage { get; }
    bool IsSpanish { get; }
    void SetLanguage(string languageCode);

    /// <summary>
    /// Signal that notifies subscribers when the language changes.
    /// Components can subscribe via LanguageSignal.OnChange(StateHasChanged)
    /// </summary>
    Signal<string> LanguageSignal { get; }

    /// <summary>
    /// Gets the localized value based on the current language setting.
    /// Returns the translation if available, otherwise falls back to the default language.
    /// </summary>
    string GetLocalizedValue(IEnumerable<LocaleValue> values);
}
