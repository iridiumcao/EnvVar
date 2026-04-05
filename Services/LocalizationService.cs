using System.Windows;

namespace EnvVar.Services;

public static class LocalizationService
{
    private const string LanguageDictionaryPrefix = "Resources/Languages/Strings.";
    private static string _currentLanguage = "zh-CN";

    public static string CurrentLanguage => _currentLanguage;

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
