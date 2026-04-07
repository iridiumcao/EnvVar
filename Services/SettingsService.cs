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
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
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
}
