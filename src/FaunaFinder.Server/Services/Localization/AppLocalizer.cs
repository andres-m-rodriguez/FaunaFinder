using BlazingSingularity.Signals;
using FaunaFinder.i18n.Contracts;

namespace FaunaFinder.Server.Services.Localization;

public sealed class AppLocalizer : IAppLocalizer
{
    private IReadOnlyDictionary<string, string> _currentTranslations = Translations.English;
    private readonly Signal<string> _languageSignal = new();

    public string CurrentLanguage { get; private set; } = "en-US";

    public bool IsSpanish => CurrentLanguage.StartsWith("es");

    public Signal<string> LanguageSignal => _languageSignal;

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

        _languageSignal.Notify(languageCode);
    }

    public string GetLocalizedValue(IEnumerable<LocaleValue> values)
    {
        var targetLocale = IsSpanish ? SupportedLocales.Spanish : SupportedLocales.English;
        var value = values.FirstOrDefault(v => v.Code.StartsWith(targetLocale));
        return value?.Value ?? values.FirstOrDefault()?.Value ?? string.Empty;
    }
}
