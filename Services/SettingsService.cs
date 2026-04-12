using System.IO;
using System.Text.Json;
using EnvVar.Models;

namespace EnvVar.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "EnvVar",
        "settings.json");

    private static AppSettings? _current;

    public static AppSettings Current
    {
        get
        {
            if (_current == null)
            {
                _current = Load();
            }
            return _current;
        }
    }

    public static AppSettings Load()
    {
        if (!File.Exists(FilePath))
        {
            return Normalize(new AppSettings());
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions) ?? new AppSettings();
            return Normalize(settings);
        }
        catch
        {
            return Normalize(new AppSettings());
        }
    }

    public static void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(Current, SerializerOptions);
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            // Ignore write errors
        }
    }

    private static AppSettings Normalize(AppSettings settings)
    {
        settings.Language = string.IsNullOrWhiteSpace(settings.Language) ? "en-US" : settings.Language;
        settings.Theme = string.IsNullOrWhiteSpace(settings.Theme) ? "System" : settings.Theme;
        settings.MaxHistoryCount = Math.Clamp(settings.MaxHistoryCount, 0, 10);
        settings.LogLevel = Enum.IsDefined(settings.LogLevel) ? settings.LogLevel : AppLogLevel.Information;
        settings.LogRetentionDays = settings.LogRetentionDays is 0 or 7 or 14 or 30 or 180 ? settings.LogRetentionDays : 14;
        settings.MaxLogFileSizeMb = Math.Clamp(settings.MaxLogFileSizeMb, 1, 1024);
        return settings;
    }
}
