namespace WikipediaPlaywrightTests.Models;

public sealed class ThemeState
{
    public string BackgroundColor { get; init; } = string.Empty;

    public string TextColor { get; init; } = string.Empty;

    public string? ThemeAttribute { get; init; }
}
