using System.Windows;
using EnvVar.Models;
using EnvVar.Services;

namespace EnvVar.Views;

public partial class LoggingSettingsWindow : Window
{
    private sealed record Option<T>(T Value, string Label);

    public LoggingSettingsWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => ThemeService.UpdateTitleBar(this);
        InitializeOptions();

        var settings = SettingsService.Current;
        LogLevelComboBox.SelectedValue = settings.LogLevel;
        LogRetentionComboBox.SelectedValue = settings.LogRetentionDays;
        MaxLogFileSizeTextBox.Text = settings.MaxLogFileSizeMb.ToString();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var fileSizeValid = int.TryParse(MaxLogFileSizeTextBox.Text, out var maxLogFileSizeMb) && maxLogFileSizeMb > 0;
        if (!fileSizeValid)
        {
            ThemedMessageBox.Show(
                this,
                LocalizationService.Get("Msg_InvalidLogFileSize"),
                LocalizationService.Get("Title_LoggingSettings"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (LogLevelComboBox.SelectedValue is not AppLogLevel logLevel ||
            LogRetentionComboBox.SelectedValue is not int retentionDays)
        {
            ThemedMessageBox.Show(
                this,
                LocalizationService.Get("Msg_InvalidLoggingSetting"),
                LocalizationService.Get("Title_LoggingSettings"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        SettingsService.Current.LogLevel = logLevel;
        SettingsService.Current.LogRetentionDays = retentionDays;
        SettingsService.Current.MaxLogFileSizeMb = maxLogFileSizeMb;
        SettingsService.Save();
        LoggingService.Shared.EnsureLogDirectoryExists();
        LoggingService.Shared.Information(
            "Logging settings updated.",
            action: "Update Logging Settings",
            context: new Dictionary<string, string?>
            {
                ["LogLevel"] = logLevel.ToString(),
                ["LogRetentionDays"] = retentionDays == 0 ? "Unlimited" : retentionDays.ToString(),
                ["MaxLogFileSizeMb"] = maxLogFileSizeMb.ToString()
            });

        DialogResult = true;
        Close();
    }

    private void InitializeOptions()
    {
        LogLevelComboBox.ItemsSource = new[]
        {
            new Option<AppLogLevel>(AppLogLevel.Error, LocalizationService.Get("Settings_LogLevel_Error")),
            new Option<AppLogLevel>(AppLogLevel.Warning, LocalizationService.Get("Settings_LogLevel_Warning")),
            new Option<AppLogLevel>(AppLogLevel.Information, LocalizationService.Get("Settings_LogLevel_Information")),
            new Option<AppLogLevel>(AppLogLevel.Debug, LocalizationService.Get("Settings_LogLevel_Debug"))
        };
        LogLevelComboBox.DisplayMemberPath = nameof(Option<AppLogLevel>.Label);
        LogLevelComboBox.SelectedValuePath = nameof(Option<AppLogLevel>.Value);

        LogRetentionComboBox.ItemsSource = new[]
        {
            new Option<int>(7, LocalizationService.Get("Settings_LogRetention_7")),
            new Option<int>(14, LocalizationService.Get("Settings_LogRetention_14")),
            new Option<int>(30, LocalizationService.Get("Settings_LogRetention_30")),
            new Option<int>(180, LocalizationService.Get("Settings_LogRetention_180")),
            new Option<int>(0, LocalizationService.Get("Settings_LogRetention_Unlimited"))
        };
        LogRetentionComboBox.DisplayMemberPath = nameof(Option<int>.Label);
        LogRetentionComboBox.SelectedValuePath = nameof(Option<int>.Value);
    }
}
