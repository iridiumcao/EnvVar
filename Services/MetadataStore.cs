using System.Text.Json;
using EnvVar.Models;

namespace EnvVar.Services;

public sealed class MetadataStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public MetadataStore(string? filePath = null)
    {
        FilePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EnvVar",
            "metadata.json");
    }

    public string FilePath { get; }

    public Dictionary<string, VariableMetadata> Load()
    {
        if (!File.Exists(FilePath))
        {
            return new Dictionary<string, VariableMetadata>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<string, VariableMetadata>(StringComparer.OrdinalIgnoreCase);
            }

            var metadata = JsonSerializer.Deserialize<Dictionary<string, VariableMetadata>>(json, SerializerOptions);
            return metadata is null
                ? new Dictionary<string, VariableMetadata>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, VariableMetadata>(metadata, StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"无法读取元数据文件：{FilePath}。请检查 JSON 格式。", ex);
        }
    }

    public void Save(IReadOnlyDictionary<string, VariableMetadata> metadata)
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("元数据文件路径无效。");
        }

        Directory.CreateDirectory(directory);
        var json = JsonSerializer.Serialize(metadata, SerializerOptions);
        File.WriteAllText(FilePath, json);
    }

    public static string BuildKey(string name, EnvironmentVariableLevel level)
    {
        return $"{name}@{level}";
    }
}
