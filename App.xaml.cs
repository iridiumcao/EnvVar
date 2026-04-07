using System.Windows;
using EnvVar.Services;

namespace EnvVar;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        ThemeService.Initialize();

        var savedLanguage = LocalizationService.LoadSavedLanguage();
        if (savedLanguage != "en-US")
        {
            LocalizationService.SwitchLanguage(savedLanguage);
        }
    }
}
