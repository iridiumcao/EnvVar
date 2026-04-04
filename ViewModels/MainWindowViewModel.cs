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
    private DisplayMode _displayMode = DisplayMode.Merged;
    private EnvironmentVariableEntry? _selectedVariable;
    private string _statusMessage = "准备就绪。";

    public MainWindowViewModel()
        : this(new EnvironmentVariableService())
    {
    }

    public MainWindowViewModel(EnvironmentVariableService environmentVariableService)
    {
        _environmentVariableService = environmentVariableService;
        Variables = new ObservableCollection<EnvironmentVariableEntry>();
        VariablesView = CollectionViewSource.GetDefaultView(Variables);
        Editor = new VariableEditorModel();
        Editor.ResetForNew();
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
                    StatusMessage = $"已选择 {value.Name}@{value.Level}。";
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
            StatusMessage = "未读取到环境变量，可直接新建。";
        }
        else
        {
            StatusMessage = $"已加载 {Variables.Count} 个环境变量。";
        }

        OnPropertyChanged(nameof(CanDeleteCurrent));
    }

    public void StartCreateNew()
    {
        SelectedVariable = null;
        Editor.ResetForNew();
        StatusMessage = "请输入新环境变量信息。";
        OnPropertyChanged(nameof(CanDeleteCurrent));
    }

    public void CancelEditing()
    {
        if (SelectedVariable is not null)
        {
            Editor.LoadFrom(SelectedVariable);
            StatusMessage = "已恢复当前选中项的原始内容。";
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
        _environmentVariableService.Save(Editor);

        var savedName = Editor.Name.Trim();
        var savedLevel = Editor.Level;

        LoadVariables(savedName, savedLevel);
        StatusMessage = $"已保存 {savedName}@{savedLevel}。";
    }

    public void DeleteCurrent()
    {
        if (Editor.IsNew)
        {
            throw new InvalidOperationException("当前是新建状态，没有可删除的环境变量。");
        }

        var name = Editor.OriginalName;
        var level = Editor.OriginalLevel;

        _environmentVariableService.Delete(name, level);
        SelectedVariable = null;
        LoadVariables();
        StatusMessage = $"已删除 {name}@{level}。";
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
