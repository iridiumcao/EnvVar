namespace EnvVar.Models;

public sealed class AppSettings
{
    public string Language { get; set; } = "en-US";
    public int MaxHistoryCount { get; set; } = 5;
    public string Theme { get; set; } = "System"; // Light, Dark, System
    public AppLogLevel LogLevel { get; set; } = AppLogLevel.Information;
    public int LogRetentionDays { get; set; } = 14;
    public int MaxLogFileSizeMb { get; set; } = 5;
}
