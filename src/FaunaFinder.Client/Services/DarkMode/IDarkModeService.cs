namespace FaunaFinder.Client.Services.DarkMode;

public interface IDarkModeService
{
    bool IsDarkMode { get; }
    void SetDarkMode(bool isDark);
    void ToggleDarkMode();
    event Action? OnDarkModeChanged;
}
