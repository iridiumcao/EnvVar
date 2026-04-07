using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using EnvVar.Models;

namespace EnvVar.Services;

public static class ThemeService
{
    private const string ThemeDictionaryPrefix = "Resources/Themes/Theme.";
    private const string DefaultTheme = "Light";

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public static string CurrentTheme => SettingsService.Current.Theme;

    public static void Initialize()
    {
        try
        {
            ApplyTheme(CurrentTheme);
            
            SystemEvents.UserPreferenceChanged += (s, e) =>
            {
                try
                {
                    if (e.Category == UserPreferenceCategory.General && CurrentTheme == "System")
                    {
                        ApplyTheme("System");
                    }
                }
                catch
                {
                    // Ignore background preference change errors
                }
            };
        }
        catch
        {
            // Fallback to Light theme if initialization fails
            try { ApplyTheme(DefaultTheme); } catch { }
        }
    }

    public static void ApplyTheme(string themeName)
    {
        if (Application.Current == null) return;

        string targetTheme = themeName;
        bool isDark = false;
        if (themeName == "System")
        {
            isDark = IsSystemDark();
            targetTheme = isDark ? "Dark" : "Light";
        }
        else
        {
            isDark = themeName == "Dark";
        }

        try
        {
            var dictUri = new Uri($"{ThemeDictionaryPrefix}{targetTheme}.xaml", UriKind.Relative);
            var newDict = new ResourceDictionary { Source = dictUri };

            var mergedDicts = Application.Current.Resources.MergedDictionaries;

            ResourceDictionary? existing = null;
            foreach (var dict in mergedDicts)
            {
                if (dict.Source != null && dict.Source.OriginalString.Contains(ThemeDictionaryPrefix))
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
            
            SettingsService.Current.Theme = themeName;
            SettingsService.Save();

            // Update all open windows title bars
            foreach (Window window in Application.Current.Windows)
            {
                UpdateTitleBar(window, isDark);
            }
        }
        catch (Exception)
        {
            if (themeName != DefaultTheme)
            {
                ApplyTheme(DefaultTheme);
            }
        }
    }

    public static void UpdateTitleBar(Window window)
    {
        bool isDark = false;
        if (CurrentTheme == "System")
        {
            isDark = IsSystemDark();
        }
        else
        {
            isDark = CurrentTheme == "Dark";
        }
        UpdateTitleBar(window, isDark);
    }

    private static void UpdateTitleBar(Window window, bool isDark)
    {
        try
        {
            IntPtr hWnd = new WindowInteropHelper(window).EnsureHandle();
            int attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
            if (IsWindows10Before20H1())
            {
                attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            }

            int useImmersiveDarkMode = isDark ? 1 : 0;
            DwmSetWindowAttribute(hWnd, attribute, ref useImmersiveDarkMode, sizeof(int));
        }
        catch
        {
            // Ignore title bar update errors on older Windows versions
        }
    }

    private static bool IsWindows10Before20H1()
    {
        // Simple version check, can be more robust if needed
        return Environment.OSVersion.Version.Major == 10 && Environment.OSVersion.Version.Build < 18985;
    }

    private static bool IsSystemDark()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key == null) return false;
            var value = key.GetValue("AppsUseLightTheme");
            return value is int i && i == 0;
        }
        catch
        {
            return false;
        }
    }
}
