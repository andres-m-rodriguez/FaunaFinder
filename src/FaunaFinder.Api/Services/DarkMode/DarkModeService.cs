namespace FaunaFinder.Api.Services.DarkMode;

public sealed class DarkModeService : IDarkModeService
{
    public bool IsDarkMode { get; private set; }

    public event Action? OnDarkModeChanged;

    public void SetDarkMode(bool isDark)
    {
        if (isDark == IsDarkMode) return;

        IsDarkMode = isDark;
        OnDarkModeChanged?.Invoke();
    }

    public void ToggleDarkMode()
    {
        SetDarkMode(!IsDarkMode);
    }
}
