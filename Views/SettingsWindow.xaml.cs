using System.Windows;
using EnvVar.Services;

namespace EnvVar.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        Loaded += (s, e) => ThemeService.UpdateTitleBar(this);
        HistoryCountTextBox.Text = SettingsService.Current.MaxHistoryCount.ToString();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(HistoryCountTextBox.Text, out var count) && count >= 0 && count <= 10)
        {
            SettingsService.Current.MaxHistoryCount = count;
            SettingsService.Save();
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show(LocalizationService.Get("Msg_InvalidHistoryCount"), 
                            LocalizationService.Get("Title_Settings"), 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
        }
    }
}
