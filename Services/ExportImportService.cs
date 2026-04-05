using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnvVar.Models;

namespace EnvVar.Services;

public sealed class ExportImportService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly EnvironmentVariableService _envService;

    public ExportImportService(EnvironmentVariableService envService)
    {
        _envService = envService;
    }

    public void Export(string filePath)
    {
        var variables = _envService.LoadAll();
        var exportData = new ExportData
        {
            ExportedAt = DateTime.UtcNow,
            Variables = variables.Select(v => new ExportVariable
            {
                Name = v.Name,
                Value = v.Value,
                Level = v.Level,
                Alias = v.Alias,
                Description = v.Description
            }).ToList()
        };

        var json = JsonSerializer.Serialize(exportData, SerializerOptions);
        File.WriteAllText(filePath, json);
    }

    public List<ExportVariable> LoadImportFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<ExportData>(json, SerializerOptions);
        return data?.Variables ?? new List<ExportVariable>();
    }

    public int Import(List<ExportVariable> variables)
    {
        var count = 0;
        foreach (var v in variables)
        {
            var editor = new VariableEditorModel();
            editor.ResetForNew();
            editor.Name = v.Name;
            editor.Value = v.Value;
            editor.Level = v.Level;
            editor.Alias = v.Alias;
            editor.Description = v.Description;

            _envService.Save(editor);
            count++;
        }

        return count;
    }
}

public sealed class ExportData
{
    public DateTime ExportedAt { get; set; }
    public List<ExportVariable> Variables { get; set; } = new();
}

public sealed class ExportVariable
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public EnvironmentVariableLevel Level { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
