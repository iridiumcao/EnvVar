using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using EnvVar.Infrastructure;
using EnvVar.Models;
using EnvVar.Services;

namespace EnvVar.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly EnvironmentVariableService _environmentVariableService;
    private readonly ExportImportService _exportImportService;
    private readonly VersionHistoryService _historyService = new();
    private DisplayMode _displayMode = DisplayMode.Merged;
    private EnvironmentVariableEntry? _selectedVariable;
    private string _statusMessage = string.Empty;
    private string _searchText = string.Empty;

    public MainWindowViewModel()
        : this(new EnvironmentVariableService())
    {
    }

    public MainWindowViewModel(EnvironmentVariableService environmentVariableService)
    {
        _environmentVariableService = environmentVariableService;
        _exportImportService = new ExportImportService(environmentVariableService);
        Variables = new ObservableCollection<EnvironmentVariableEntry>();
        VariablesView = CollectionViewSource.GetDefaultView(Variables);
        VariablesView.Filter = FilterVariable;
        Editor = new VariableEditorModel();
        Editor.ResetForNew();
        _statusMessage = LocalizationService.Get("Msg_Ready");
        ApplyDisplayMode();
    }

    public ObservableCollection<EnvironmentVariableEntry> Variables { get; }

    public ICollectionView VariablesView { get; }

    public VariableEditorModel Editor { get; }

    public string MetadataPath => _environmentVariableService.MetadataPath;

    public DisplayMode DisplayMode
    {
        get => _displayMode;
        set
        {
            if (SetProperty(ref _displayMode, value))
            {
                ApplyDisplayMode();
            }
        }
    }

    public EnvironmentVariableEntry? SelectedVariable
    {
        get => _selectedVariable;
        set
        {
            if (SetProperty(ref _selectedVariable, value))
            {
                if (value is not null)
                {
                    Editor.LoadFrom(value);
                    StatusMessage = LocalizationService.Get("Msg_Selected", value.Name, value.Level);
                }

                OnPropertyChanged(nameof(HasSelectedVariable));
                OnPropertyChanged(nameof(CanDeleteCurrent));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                VariablesView.Refresh();
            }
        }
    }

    public bool HasSelectedVariable => SelectedVariable is not null;

    public bool CanDeleteCurrent => !Editor.IsNew;

    public void LoadVariables(string? preferredName = null, EnvironmentVariableLevel? preferredLevel = null)
    {
        var preferredKey = preferredName is null || preferredLevel is null
            ? null
            : MetadataStore.BuildKey(preferredName, preferredLevel.Value);

        var items = _environmentVariableService.LoadAll();
        Variables.Clear();

        foreach (var item in items)
        {
            Variables.Add(item);
        }

        ApplyDisplayMode();

        if (!string.IsNullOrWhiteSpace(preferredKey))
        {
            SelectedVariable = Variables.FirstOrDefault(item =>
                string.Equals(item.CompositeKey, preferredKey, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedVariable is null)
        {
            SelectedVariable = Variables.FirstOrDefault();
        }

        if (SelectedVariable is null)
        {
            Editor.ResetForNew();
            StatusMessage = LocalizationService.Get("Msg_NoVariables");
        }
        else
        {
            StatusMessage = LocalizationService.Get("Msg_Loaded", Variables.Count);
        }

        OnPropertyChanged(nameof(CanDeleteCurrent));
    }

    public void StartCreateNew()
    {
        SelectedVariable = null;
        Editor.ResetForNew();
        StatusMessage = LocalizationService.Get("Msg_NewVariable");
        OnPropertyChanged(nameof(CanDeleteCurrent));
    }

    public void CancelEditing()
    {
        if (SelectedVariable is not null)
        {
            Editor.LoadFrom(SelectedVariable);
            StatusMessage = LocalizationService.Get("Msg_Restored");
            return;
        }

        if (Variables.Count > 0)
        {
            SelectedVariable = Variables[0];
            return;
        }

        StartCreateNew();
    }

    public bool WouldOverwriteExisting()
    {
        var trimmedName = Editor.Name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return false;
        }

        return Variables.Any(item =>
            item.Level == Editor.Level &&
            string.Equals(item.Name, trimmedName, StringComparison.OrdinalIgnoreCase) &&
            (Editor.IsNew ||
             !string.Equals(item.Name, Editor.OriginalName, StringComparison.OrdinalIgnoreCase) ||
             item.Level != Editor.OriginalLevel));
    }

    public void SaveCurrent()
    {
        if (!Editor.IsNew)
        {
            var existing = Variables.FirstOrDefault(v =>
                string.Equals(v.Name, Editor.OriginalName, StringComparison.OrdinalIgnoreCase)
                && v.Level == Editor.OriginalLevel);

            if (existing is not null && !string.Equals(existing.Value, Editor.Value, StringComparison.Ordinal))
            {
                _historyService.RecordHistory(existing.Name, existing.Level, existing.Value);
            }
        }

        _environmentVariableService.Save(Editor);

        var savedName = Editor.Name.Trim();
        var savedLevel = Editor.Level;

        LoadVariables(savedName, savedLevel);
        StatusMessage = LocalizationService.Get("Msg_Saved", savedName, savedLevel);
    }

    public void DeleteCurrent()
    {
        if (Editor.IsNew)
        {
            throw new InvalidOperationException(LocalizationService.Get("Msg_CannotDeleteNew"));
        }

        var name = Editor.OriginalName;
        var level = Editor.OriginalLevel;

        var existing = Variables.FirstOrDefault(v =>
            string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase)
            && v.Level == level);

        if (existing is not null && !string.IsNullOrEmpty(existing.Value))
        {
            _historyService.RecordHistory(name, level, existing.Value);
        }

        _environmentVariableService.Delete(name, level);
        SelectedVariable = null;
        LoadVariables();
        StatusMessage = LocalizationService.Get("Msg_Deleted", name, level);
    }

    public void Export(string filePath)
    {
        _exportImportService.Export(filePath);
    }

    public List<ExportVariable> LoadImportFile(string filePath)
    {
        return _exportImportService.LoadImportFile(filePath);
    }

    public int Import(List<ExportVariable> variables)
    {
        return _exportImportService.Import(variables);
    }

    public void ApplyColumnSort(string propertyName, ListSortDirection direction)
    {
        using (VariablesView.DeferRefresh())
        {
            VariablesView.SortDescriptions.Clear();

            if (DisplayMode == DisplayMode.Grouped)
            {
                VariablesView.SortDescriptions.Add(
                    new SortDescription(nameof(EnvironmentVariableEntry.Level), ListSortDirection.Ascending));
            }

            VariablesView.SortDescriptions.Add(new SortDescription(propertyName, direction));
        }
    }

    public void ResetColumnSort()
    {
        ApplyDisplayMode();
    }

    public IReadOnlyList<VariableHistoryEntry> GetCurrentVariableHistory()
    {
        if (Editor.IsNew || string.IsNullOrWhiteSpace(Editor.OriginalName))
        {
            return [];
        }

        return _historyService.GetHistory(Editor.OriginalName, Editor.OriginalLevel);
    }

    public void RestoreFromHistory(VariableHistoryEntry entry)
    {
        Editor.Value = entry.Value;
        StatusMessage = LocalizationService.Get("Msg_HistoryRestored");
    }

    private bool FilterVariable(object obj)
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            return true;
        }

        if (obj is not EnvironmentVariableEntry entry)
        {
            return false;
        }

        var search = _searchText.Trim();
        return entry.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || entry.Alias.Contains(search, StringComparison.OrdinalIgnoreCase)
            || entry.Value.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyDisplayMode()
    {
        using (VariablesView.DeferRefresh())
        {
            VariablesView.SortDescriptions.Clear();
            VariablesView.GroupDescriptions.Clear();

            VariablesView.SortDescriptions.Add(new SortDescription(nameof(EnvironmentVariableEntry.Level), ListSortDirection.Ascending));
            VariablesView.SortDescriptions.Add(new SortDescription(nameof(EnvironmentVariableEntry.Name), ListSortDirection.Ascending));

            if (DisplayMode == DisplayMode.Grouped)
            {
                VariablesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(EnvironmentVariableEntry.Level)));
            }
        }
    }
}
