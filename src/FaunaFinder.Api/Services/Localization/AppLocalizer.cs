using FaunaFinder.i18n.Contracts;

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

    public string GetLocalizedValue(IEnumerable<LocaleValue> values)
    {
        var targetLocale = IsSpanish ? SupportedLocales.Spanish : SupportedLocales.English;
        var value = values.FirstOrDefault(v => v.Code.StartsWith(targetLocale));
        return value?.Value ?? values.FirstOrDefault()?.Value ?? string.Empty;
    }
}
