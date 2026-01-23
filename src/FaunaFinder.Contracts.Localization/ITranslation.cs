namespace FaunaFinder.Contracts.Localization;

/// <summary>
/// Contract for a translation entry.
/// </summary>
public interface ITranslation
{
    /// <summary>
    /// The language code this translation belongs to.
    /// </summary>
    string LanguageCode { get; }

    /// <summary>
    /// The translation key (e.g., "Home_ClickMunicipality").
    /// </summary>
    string Key { get; }

    /// <summary>
    /// The translated value (e.g., "Click on a municipality...").
    /// </summary>
    string Value { get; }
}
