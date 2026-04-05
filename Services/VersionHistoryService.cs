using System.IO;
using EnvVar.Models;

namespace EnvVar.Services;

public class VersionHistoryService
{
    private readonly string _snapshotDirectory;

    public VersionHistoryService()
    {
        _snapshotDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EnvVar",
            "Snapshots");

        Directory.CreateDirectory(_snapshotDirectory);
    }

    public void SaveSnapshot(IEnumerable<EnvironmentVariableEntry> variables)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"snapshot_{timestamp}.json";
        var filePath = Path.Combine(_snapshotDirectory, fileName);

        var exportVariables = variables.Select(v => new ExportVariable
        {
            Name = v.Name,
            Value = v.Value,
            Level = v.Level,
            Alias = v.Alias
        }).ToList();

        var json = System.Text.Json.JsonSerializer.Serialize(exportVariables, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(filePath, json);
    }

    public List<SnapshotInfo> GetSnapshots()
    {
        if (!Directory.Exists(_snapshotDirectory))
        {
            return new List<SnapshotInfo>();
        }

        var files = Directory.GetFiles(_snapshotDirectory, "snapshot_*.json")
            .OrderByDescending(f => f);

        var snapshots = new List<SnapshotInfo>();
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            snapshots.Add(new SnapshotInfo
            {
                FilePath = file,
                FileName = fileInfo.Name,
                CreatedDate = fileInfo.CreationTime
            });
        }

        return snapshots;
    }

    public List<ExportVariable> LoadSnapshot(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return System.Text.Json.JsonSerializer.Deserialize<List<ExportVariable>>(json) ?? new List<ExportVariable>();
    }
}

public class SnapshotInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string DisplayName => $"{FileName} ({CreatedDate:g})";
}
