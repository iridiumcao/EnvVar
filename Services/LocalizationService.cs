using System;
using System.IO;
using System.Windows;

namespace EnvVar.Services;

public static class LocalizationService
{
    private const string LanguageDictionaryPrefix = "Resources/Languages/Strings.";
    private const string DefaultLanguage = "en-US";

    public static event EventHandler<string>? LanguageChanged;

    private static string _currentLanguage = DefaultLanguage;

    public static string CurrentLanguage => _currentLanguage;

    public static string LoadSavedLanguage()
    {
        var lang = SettingsService.Current.Language;
        if (!string.IsNullOrEmpty(lang))
        {
            _currentLanguage = lang;
            return lang;
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
        
        SettingsService.Current.Language = cultureCode;
        SettingsService.Save();

        LanguageChanged?.Invoke(null, cultureCode);
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
