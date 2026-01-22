namespace FaunaFinder.Api.Services.Localization;

public interface IAppLocalizer
{
    string this[string key] { get; }
    string this[string key, params object[] args] { get; }
    string CurrentLanguage { get; }
    bool IsSpanish { get; }
    void SetLanguage(string languageCode);
    event Action? OnLanguageChanged;
}
