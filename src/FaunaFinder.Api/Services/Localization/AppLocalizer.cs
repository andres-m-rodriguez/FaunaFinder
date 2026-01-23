using FaunaFinder.Contracts.Localization;
using FaunaFinder.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FaunaFinder.Api.Services.Localization;

public sealed class AppLocalizer : IAppLocalizer, ILocalizer
{
    private readonly IDbContextFactory<FaunaFinderContext> _dbFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AppLocalizer> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    private IReadOnlyDictionary<string, string> _currentTranslations;
    private IReadOnlyList<ILanguage>? _availableLanguages;

    public AppLocalizer(
        IDbContextFactory<FaunaFinderContext> dbFactory,
        IMemoryCache cache,
        ILogger<AppLocalizer> logger)
    {
        _dbFactory = dbFactory;
        _cache = cache;
        _logger = logger;
        _currentTranslations = LoadTranslationsFromCache("en");
    }

    public string CurrentLanguage { get; private set; } = "en";

    public bool IsSpanish => CurrentLanguage.StartsWith("es");

    public event Action? OnLanguageChanged;

    public string this[string key] =>
        _currentTranslations.TryGetValue(key, out var value) ? value : key;

    public string this[string key, params object[] args] =>
        string.Format(this[key], args);

    public void SetLanguage(string languageCode)
    {
        var normalizedCode = languageCode.StartsWith("es") ? "es" : "en";
        if (normalizedCode == CurrentLanguage) return;

        CurrentLanguage = normalizedCode;
        _currentTranslations = LoadTranslationsFromCache(normalizedCode);

        OnLanguageChanged?.Invoke();
    }

    public IReadOnlyList<ILanguage> GetAvailableLanguages()
    {
        if (_availableLanguages is not null)
            return _availableLanguages;

        _availableLanguages = _cache.GetOrCreate("languages", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
            return LoadLanguagesFromDatabase();
        }) ?? GetFallbackLanguages();

        return _availableLanguages;
    }

    private IReadOnlyDictionary<string, string> LoadTranslationsFromCache(string languageCode)
    {
        var cacheKey = $"translations_{languageCode}";

        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
            return LoadTranslationsFromDatabase(languageCode);
        }) ?? GetFallbackTranslations(languageCode);
    }

    private IReadOnlyDictionary<string, string> LoadTranslationsFromDatabase(string languageCode)
    {
        try
        {
            using var context = _dbFactory.CreateDbContext();

            var translations = context.Translations
                .Include(t => t.Language)
                .Where(t => t.Language.Code == languageCode)
                .ToDictionary(t => t.Key, t => t.Value);

            if (translations.Count == 0)
            {
                _logger.LogWarning("No translations found in database for language {LanguageCode}, using fallback", languageCode);
                return GetFallbackTranslations(languageCode);
            }

            _logger.LogDebug("Loaded {Count} translations from database for language {LanguageCode}", translations.Count, languageCode);
            return translations;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load translations from database for language {LanguageCode}, using fallback", languageCode);
            return GetFallbackTranslations(languageCode);
        }
    }

    private IReadOnlyList<ILanguage> LoadLanguagesFromDatabase()
    {
        try
        {
            using var context = _dbFactory.CreateDbContext();
            return context.Languages.ToList<ILanguage>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load languages from database, using fallback");
            return GetFallbackLanguages();
        }
    }

    private static IReadOnlyDictionary<string, string> GetFallbackTranslations(string languageCode)
    {
        return languageCode == "es" ? Translations.Spanish : Translations.English;
    }

    private static IReadOnlyList<ILanguage> GetFallbackLanguages()
    {
        return
        [
            new FallbackLanguage("en", "English", true),
            new FallbackLanguage("es", "Español", false)
        ];
    }

    private sealed record FallbackLanguage(string Code, string Name, bool IsDefault) : ILanguage;
}
