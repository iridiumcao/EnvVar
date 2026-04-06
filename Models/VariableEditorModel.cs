using System.Collections.ObjectModel;
using System.ComponentModel;
using EnvVar.Infrastructure;
using EnvVar.Services;
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
    private bool _isWellKnown;
    private string _value = string.Empty;
    private bool _syncingFromEditable;
    private readonly ObservableCollection<EditableValueItem> _editableValues = new();

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
                if (IsNew)
                {
                    var desc = WellKnownVariables.GetDescription(value);
                    if (!string.IsNullOrEmpty(desc))
                    {
                        Description = desc;
                        IsWellKnown = true;
                    }
                    else if (IsWellKnown)
                    {
                        Description = string.Empty;
                        IsWellKnown = false;
                    }
                }
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
        set
        {
            if (SetProperty(ref _description, value))
            {
                // If the value being set doesn't match current language's well-known description,
                // it's a custom description.
                if (IsWellKnown && value != WellKnownVariables.GetDescription(Name))
                {
                    IsWellKnown = false;
                }
            }
        }
    }

    public bool IsWellKnown
    {
        get => _isWellKnown;
        set => SetProperty(ref _isWellKnown, value);
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
                if (!_syncingFromEditable)
                {
                    RebuildEditableValues();
                }
            }
        }
    }

    public bool CanDelete => !IsNew;

    public bool IsMultiValue => EnvironmentVariableValueParser.IsMultiValue(Value);

    public IReadOnlyList<string> SplitValues => EnvironmentVariableValueParser.Split(Value);

    public ObservableCollection<EditableValueItem> EditableValues => _editableValues;

    public string Header => IsNew
        ? LocalizationService.Get("Editor_NewHeader")
        : LocalizationService.Get("Editor_EditHeader", Name, Level);

    public void NotifyHeaderChanged()
    {
        OnPropertyChanged(nameof(Header));
    }

    public void LoadFrom(EnvironmentVariableEntry entry)
    {
        IsNew = false;
        OriginalName = entry.Name;
        OriginalLevel = entry.Level;
        Name = entry.Name;
        Level = entry.Level;
        Alias = entry.Alias;
        Description = entry.Description;
        IsWellKnown = entry.IsWellKnown;
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
        IsWellKnown = false;
        Value = string.Empty;
    }

    public void AddValueItem(int index)
    {
        var item = new EditableValueItem(string.Empty, 0);
        item.PropertyChanged += EditableValueItem_Changed;
        if (index < 0 || index > _editableValues.Count)
        {
            _editableValues.Add(item);
        }
        else
        {
            _editableValues.Insert(index, item);
        }

        RefreshIndices();
        SyncValueFromEditableValues();
    }

    public void RemoveValueItemAt(int index)
    {
        if (index < 0 || index >= _editableValues.Count)
        {
            return;
        }

        _editableValues[index].PropertyChanged -= EditableValueItem_Changed;
        _editableValues.RemoveAt(index);
        RefreshIndices();
        SyncValueFromEditableValues();
    }

    public void MoveValueItemUp(int index)
    {
        if (index <= 0 || index >= _editableValues.Count)
        {
            return;
        }

        _editableValues.Move(index, index - 1);
        RefreshIndices();
        SyncValueFromEditableValues();
    }

    public void MoveValueItemDown(int index)
    {
        if (index < 0 || index >= _editableValues.Count - 1)
        {
            return;
        }

        _editableValues.Move(index, index + 1);
        RefreshIndices();
        SyncValueFromEditableValues();
    }

    public void SortValuesAscending()
    {
        var sorted = _editableValues.Select(v => v.Value)
            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToList();
        ApplyNewOrder(sorted);
    }

    public void SortValuesDescending()
    {
        var sorted = _editableValues.Select(v => v.Value)
            .OrderByDescending(v => v, StringComparer.OrdinalIgnoreCase).ToList();
        ApplyNewOrder(sorted);
    }

    private void ApplyNewOrder(List<string> values)
    {
        foreach (var item in _editableValues)
        {
            item.PropertyChanged -= EditableValueItem_Changed;
        }

        _editableValues.Clear();
        int i = 0;
        foreach (var val in values)
        {
            var item = new EditableValueItem(val, i++);
            item.PropertyChanged += EditableValueItem_Changed;
            _editableValues.Add(item);
        }

        SyncValueFromEditableValues();
    }

    private void RefreshIndices()
    {
        for (int i = 0; i < _editableValues.Count; i++)
        {
            _editableValues[i].Index = i;
        }
    }

    private void RebuildEditableValues()
    {
        foreach (var item in _editableValues)
        {
            item.PropertyChanged -= EditableValueItem_Changed;
        }

        _editableValues.Clear();
        if (IsMultiValue)
        {
            int i = 0;
            foreach (var val in SplitValues)
            {
                var editItem = new EditableValueItem(val, i++);
                editItem.PropertyChanged += EditableValueItem_Changed;
                _editableValues.Add(editItem);
            }
        }
    }

    public void DeduplicateValue()
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            return;
        }

        var values = EnvironmentVariableValueParser.Split(Value);
        var unique = values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        
        if (unique.Count != values.Count)
        {
            Value = string.Join(";", unique);
        }
    }

    public void CommitStructuredChanges()
    {
        // Rebuild the UI list from the current (deduplicated) Value
        RebuildEditableValues();
    }

    private void EditableValueItem_Changed(object? sender, PropertyChangedEventArgs e)
    {
        SyncValueFromEditableValues();
    }

    private void SyncValueFromEditableValues()
    {
        _syncingFromEditable = true;
        try
        {
            // Use Distinct() to remove duplicates while preserving order
            var uniqueValues = _editableValues
                .Select(v => v.Value)
                .Where(v => !string.IsNullOrEmpty(v))
                .Distinct(StringComparer.OrdinalIgnoreCase);
            Value = string.Join(";", uniqueValues);
        }
        finally
        {
            _syncingFromEditable = false;
        }
    }
}
