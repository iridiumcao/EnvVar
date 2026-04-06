using EnvVar.Infrastructure;

namespace EnvVar.Models;

public sealed class EditableValueItem : ObservableObject
{
    private string _value = string.Empty;
    private int _index;

    public EditableValueItem(string value, int index)
    {
        _value = value;
        _index = index;
    }

    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }
}
