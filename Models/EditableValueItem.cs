using EnvVar.Infrastructure;

namespace EnvVar.Models;

public sealed class EditableValueItem : ObservableObject
{
    private string _value = string.Empty;

    public EditableValueItem(string value)
    {
        _value = value;
    }

    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }
}
