namespace EnvVar.Models;

public sealed class AppSettings
{
    public string Language { get; set; } = "en-US";
    public int MaxHistoryCount { get; set; } = 5;
}
