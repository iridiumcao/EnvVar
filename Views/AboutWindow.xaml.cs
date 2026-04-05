using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace EnvVar.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = version?.ToString(3) ?? "1.0.0";
    }

    private void GitHubLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });
        e.Handled = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
