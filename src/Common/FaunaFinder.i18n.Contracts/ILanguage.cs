namespace FaunaFinder.i18n.Contracts;

/// <summary>
/// Contract for a supported language.
/// </summary>
public interface ILanguage
{
    /// <summary>
    /// Language code (e.g., "en", "es").
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Display name of the language (e.g., "English", "Español").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Whether this is the default/fallback language.
    /// </summary>
    bool IsDefault { get; }
}
