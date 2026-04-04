using EnvVar.Infrastructure;
using EnvVar.Utilities;

namespace EnvVar.Models;

public sealed class VariableEditorModel : ObservableObject
{
    private bool _isNew = true;
    private string _originalName = string.Empty;
    private EnvironmentVariableLevel _originalLevel = EnvironmentVariableLevel.User;
    private string _name = string.Empty;
    private EnvironmentVariableLevel _level = EnvironmentVariableLevel.User;
    private string _alias = string.Empty;
    private string _description = string.Empty;
    private string _value = string.Empty;

    public bool IsNew
    {
        get => _isNew;
        private set
        {
            if (SetProperty(ref _isNew, value))
            {
                OnPropertyChanged(nameof(CanDelete));
                OnPropertyChanged(nameof(Header));
            }
        }
    }

    public string OriginalName
    {
        get => _originalName;
        private set => SetProperty(ref _originalName, value);
    }

    public EnvironmentVariableLevel OriginalLevel
    {
        get => _originalLevel;
        private set => SetProperty(ref _originalLevel, value);
    }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                OnPropertyChanged(nameof(Header));
            }
        }
    }

    public EnvironmentVariableLevel Level
    {
        get => _level;
        set
        {
            if (SetProperty(ref _level, value))
            {
                OnPropertyChanged(nameof(Header));
            }
        }
    }

    public string Alias
    {
        get => _alias;
        set => SetProperty(ref _alias, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value))
            {
                OnPropertyChanged(nameof(IsMultiValue));
                OnPropertyChanged(nameof(SplitValues));
            }
        }
    }

    public bool CanDelete => !IsNew;

    public bool IsMultiValue => EnvironmentVariableValueParser.IsMultiValue(Value);

    public IReadOnlyList<string> SplitValues => EnvironmentVariableValueParser.Split(Value);

    public string Header => IsNew ? "新建环境变量" : $"编辑 {Name}@{Level}";

    public void LoadFrom(EnvironmentVariableEntry entry)
    {
        IsNew = false;
        OriginalName = entry.Name;
        OriginalLevel = entry.Level;
        Name = entry.Name;
        Level = entry.Level;
        Alias = entry.Alias;
        Description = entry.Description;
        Value = entry.Value;
    }

    public void ResetForNew()
    {
        IsNew = true;
        OriginalName = string.Empty;
        OriginalLevel = EnvironmentVariableLevel.User;
        Name = string.Empty;
        Level = EnvironmentVariableLevel.User;
        Alias = string.Empty;
        Description = string.Empty;
        Value = string.Empty;
    }
}
