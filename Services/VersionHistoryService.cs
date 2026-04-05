using System.IO;
using System.Text.Json;
using EnvVar.Models;

namespace EnvVar.Services;

public class VersionHistoryService
{
    private const int MaxHistoryPerVariable = 5;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _historyFilePath;

    public VersionHistoryService()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EnvVar");

        Directory.CreateDirectory(directory);
        _historyFilePath = Path.Combine(directory, "history.json");
    }

    public void RecordHistory(string name, EnvironmentVariableLevel level, string value)
    {
        var allHistory = LoadAll();
        var key = MetadataStore.BuildKey(name, level);

        if (!allHistory.TryGetValue(key, out var entries))
        {
            entries = [];
            allHistory[key] = entries;
        }

        entries.Insert(0, new VariableHistoryEntry
        {
            Value = value,
            Timestamp = DateTime.Now
        });

        if (entries.Count > MaxHistoryPerVariable)
        {
            entries.RemoveRange(MaxHistoryPerVariable, entries.Count - MaxHistoryPerVariable);
        }

        SaveAll(allHistory);
    }

    public IReadOnlyList<VariableHistoryEntry> GetHistory(string name, EnvironmentVariableLevel level)
    {
        var allHistory = LoadAll();
        var key = MetadataStore.BuildKey(name, level);
        return allHistory.TryGetValue(key, out var entries) ? entries : [];
    }

    public void RemoveHistory(string name, EnvironmentVariableLevel level)
    {
        var allHistory = LoadAll();
        var key = MetadataStore.BuildKey(name, level);

        if (allHistory.Remove(key))
        {
            SaveAll(allHistory);
        }
    }

    private Dictionary<string, List<VariableHistoryEntry>> LoadAll()
    {
        if (!File.Exists(_historyFilePath))
        {
            return new Dictionary<string, List<VariableHistoryEntry>>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var json = File.ReadAllText(_historyFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<string, List<VariableHistoryEntry>>(StringComparer.OrdinalIgnoreCase);
            }

            var data = JsonSerializer.Deserialize<Dictionary<string, List<VariableHistoryEntry>>>(json, SerializerOptions);
            return data is null
                ? new Dictionary<string, List<VariableHistoryEntry>>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, List<VariableHistoryEntry>>(data, StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new Dictionary<string, List<VariableHistoryEntry>>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private void SaveAll(Dictionary<string, List<VariableHistoryEntry>> allHistory)
    {
        var json = JsonSerializer.Serialize(allHistory, SerializerOptions);
        File.WriteAllText(_historyFilePath, json);
    }
}

public class VariableHistoryEntry
{
    public string Value { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
