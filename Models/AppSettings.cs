namespace EnvVar.Models;

public sealed class AppSettings
{
    public string Language { get; set; } = "en-US";
    public int MaxHistoryCount { get; set; } = 5;
    public string Theme { get; set; } = "System"; // Light, Dark, System
}
