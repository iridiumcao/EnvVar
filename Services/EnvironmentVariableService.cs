using System.Collections;
using System.Runtime.InteropServices;
using EnvVar.Models;
using Microsoft.Win32;

namespace EnvVar.Services;

public sealed class EnvironmentVariableService
{
    private const uint WmSettingChange = 0x001A;
    private static readonly IntPtr HwndBroadcast = new(0xffff);
    private readonly MetadataStore _metadataStore;

    public EnvironmentVariableService(MetadataStore? metadataStore = null)
    {
        _metadataStore = metadataStore ?? new MetadataStore();
    }

    public string MetadataPath => _metadataStore.FilePath;

    public IReadOnlyList<EnvironmentVariableEntry> LoadAll()
    {
        var metadata = _metadataStore.Load();
        var items = new List<EnvironmentVariableEntry>();

        AddVariables(EnvironmentVariableLevel.User, metadata, items);
        AddVariables(EnvironmentVariableLevel.System, metadata, items);

        return items
            .OrderBy(item => item.Level)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public bool Exists(string name, EnvironmentVariableLevel level)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var variables = Environment.GetEnvironmentVariables(level.ToTarget());
        return variables.Contains(name);
    }

    public void Save(VariableEditorModel editor)
    {
        ArgumentNullException.ThrowIfNull(editor);

        var name = editor.Name.Trim();
        ValidateName(name);

        SetRegistryVariable(name, editor.Value, editor.Level);

        if (!editor.IsNew &&
            (!string.Equals(editor.OriginalName, name, StringComparison.OrdinalIgnoreCase) ||
             editor.OriginalLevel != editor.Level))
        {
            DeleteRegistryVariable(editor.OriginalName, editor.OriginalLevel);
        }

        SaveMetadata(editor, name);
        BroadcastEnvironmentChange();
    }

    public void Delete(string name, EnvironmentVariableLevel level)
    {
        ValidateName(name);

        DeleteRegistryVariable(name, level);

        var metadata = _metadataStore.Load();
        metadata.Remove(MetadataStore.BuildKey(name, level));
        _metadataStore.Save(metadata);

        BroadcastEnvironmentChange();
    }

    private static void AddVariables(
        EnvironmentVariableLevel level,
        IReadOnlyDictionary<string, VariableMetadata> metadata,
        ICollection<EnvironmentVariableEntry> items)
    {
        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables(level.ToTarget()))
        {
            var name = entry.Key?.ToString();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var key = MetadataStore.BuildKey(name, level);
            metadata.TryGetValue(key, out var meta);

            var description = meta?.Description ?? string.Empty;
            var isWellKnown = false;
            if (string.IsNullOrEmpty(description) && meta is null)
            {
                description = WellKnownVariables.GetDescription(name) ?? string.Empty;
                isWellKnown = !string.IsNullOrEmpty(description);
            }

            items.Add(new EnvironmentVariableEntry
            {
                Name = name,
                Value = entry.Value?.ToString() ?? string.Empty,
                Alias = meta?.Alias ?? string.Empty,
                Description = description,
                IsWellKnown = isWellKnown,
                Level = level
            });
        }
    }

    private void SaveMetadata(VariableEditorModel editor, string name)
    {
        var metadata = _metadataStore.Load();
        var newKey = MetadataStore.BuildKey(name, editor.Level);

        if (!editor.IsNew)
        {
            var oldKey = MetadataStore.BuildKey(editor.OriginalName, editor.OriginalLevel);
            if (!string.Equals(oldKey, newKey, StringComparison.OrdinalIgnoreCase))
            {
                metadata.Remove(oldKey);
            }
        }

        var descriptionToSave = editor.IsWellKnown ? string.Empty : editor.Description.Trim();

        if (string.IsNullOrWhiteSpace(editor.Alias) && string.IsNullOrWhiteSpace(descriptionToSave))
        {
            metadata.Remove(newKey);
        }
        else
        {
            metadata[newKey] = new VariableMetadata
            {
                Alias = editor.Alias.Trim(),
                Description = descriptionToSave
            };
        }

        _metadataStore.Save(metadata);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(LocalizationService.Get("Msg_NameEmpty"));
        }

        if (name.Contains('='))
        {
            throw new InvalidOperationException(LocalizationService.Get("Msg_NameContainsEquals"));
        }
    }

    private static RegistryKey OpenEnvironmentKey(EnvironmentVariableLevel level, bool writable)
    {
        return level == EnvironmentVariableLevel.User
            ? Registry.CurrentUser.OpenSubKey("Environment", writable)!
            : Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", writable)!;
    }

    private static void SetRegistryVariable(string name, string value, EnvironmentVariableLevel level)
    {
        using var key = OpenEnvironmentKey(level, writable: true);
        var kind = RegistryValueKind.String;
        try { kind = key.GetValueKind(name); } catch { /* new entry */ }
        if (kind != RegistryValueKind.ExpandString && value.Contains('%'))
            kind = RegistryValueKind.ExpandString;
        key.SetValue(name, value, kind);
    }

    private static void DeleteRegistryVariable(string name, EnvironmentVariableLevel level)
    {
        using var key = OpenEnvironmentKey(level, writable: true);
        key.DeleteValue(name, throwOnMissingValue: false);
    }

    private static void BroadcastEnvironmentChange()
    {
        Task.Run(() =>
        {
            _ = SendMessageTimeout(
                HwndBroadcast,
                WmSettingChange,
                IntPtr.Zero,
                "Environment",
                SendMessageTimeoutFlags.AbortIfHung,
                5000,
                out _);
        });
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        string lParam,
        SendMessageTimeoutFlags flags,
        uint timeout,
        out IntPtr result);

    [Flags]
    private enum SendMessageTimeoutFlags : uint
    {
        AbortIfHung = 0x0002
    }
}
