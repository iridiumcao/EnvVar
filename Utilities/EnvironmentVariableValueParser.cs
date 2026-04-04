using System.Collections.ObjectModel;

namespace EnvVar.Utilities;

public static class EnvironmentVariableValueParser
{
    public static bool IsMultiValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Contains(';');
    }

    public static IReadOnlyList<string> Split(string? value)
    {
        if (!IsMultiValue(value))
        {
            return Array.Empty<string>();
        }

        return value!
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }

    public static string BuildPreview(string? value, int maxLength = 80)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var singleLine = value
            .Replace(Environment.NewLine, " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();

        return singleLine.Length <= maxLength
            ? singleLine
            : $"{singleLine[..maxLength]}...";
    }
}
