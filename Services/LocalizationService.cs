using System.IO;
using System.Windows;

namespace EnvVar.Services;

public static class LocalizationService
{
    private const string LanguageDictionaryPrefix = "Resources/Languages/Strings.";
    private const string DefaultLanguage = "en-US";

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "EnvVar",
        "language.txt");

    private static string _currentLanguage = DefaultLanguage;

    public static string CurrentLanguage => _currentLanguage;

    public static string LoadSavedLanguage()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var lang = File.ReadAllText(SettingsPath).Trim();
                if (!string.IsNullOrEmpty(lang))
                {
                    return lang;
                }
            }
        }
        catch
        {
            // Ignore read errors; fall back to default
        }

        return DefaultLanguage;
    }

    public static void SwitchLanguage(string cultureCode)
    {
        var dictUri = new Uri($"{LanguageDictionaryPrefix}{cultureCode}.xaml", UriKind.Relative);
        var newDict = new ResourceDictionary { Source = dictUri };

        var mergedDicts = Application.Current.Resources.MergedDictionaries;

        ResourceDictionary? existing = null;
        foreach (var dict in mergedDicts)
        {
            if (dict.Source != null && dict.Source.OriginalString.Contains(LanguageDictionaryPrefix))
            {
                existing = dict;
                break;
            }
        }

        if (existing != null)
        {
            mergedDicts.Remove(existing);
        }

        mergedDicts.Add(newDict);
        _currentLanguage = cultureCode;
        SaveLanguagePreference(cultureCode);
    }

    private static void SaveLanguagePreference(string cultureCode)
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (dir != null)
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(SettingsPath, cultureCode);
        }
        catch
        {
            // Ignore write errors
        }
    }

    public static string Get(string key)
    {
        if (Application.Current.Resources[key] is string s)
        {
            return s;
        }

        return key;
    }

    public static string Get(string key, params object[] args)
    {
        var template = Get(key);
        return string.Format(template, args);
    }
}
