using EnvVar.Infrastructure;
using EnvVar.Utilities;

namespace EnvVar.Models;

public sealed class EnvironmentVariableEntry : ObservableObject
{
    private string _name = string.Empty;
    private string _value = string.Empty;
    private string _alias = string.Empty;
    private string _description = string.Empty;
    private EnvironmentVariableLevel _level;

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                OnPropertyChanged(nameof(NameDisplay));
                OnPropertyChanged(nameof(CompositeKey));
            }
        }
    }

    public string Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value))
            {
                OnPropertyChanged(nameof(Preview));
                OnPropertyChanged(nameof(IsMultiValue));
                OnPropertyChanged(nameof(SplitValues));
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

    public EnvironmentVariableLevel Level
    {
        get => _level;
        set
        {
            if (SetProperty(ref _level, value))
            {
                OnPropertyChanged(nameof(CompositeKey));
            }
        }
    }

    public string CompositeKey => $"{Name}@{Level}";

    public bool IsMultiValue => EnvironmentVariableValueParser.IsMultiValue(Value);

    public IReadOnlyList<string> SplitValues => EnvironmentVariableValueParser.Split(Value);

    public string Preview => EnvironmentVariableValueParser.BuildPreview(Value);

    public string NameDisplay => IsMultiValue ? $"[LIST] {Name}" : Name;

    public EnvironmentVariableEntry Clone()
    {
        return new EnvironmentVariableEntry
        {
            Name = Name,
            Value = Value,
            Alias = Alias,
            Description = Description,
            Level = Level
        };
    }
}
