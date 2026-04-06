using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Navigation;

namespace EnvVar.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        var processPath = Environment.ProcessPath;
        var assemblyPath = AppContext.BaseDirectory;;
        var path = !string.IsNullOrEmpty(processPath) ? processPath : assemblyPath;
        var buildDate = !string.IsNullOrEmpty(path) && File.Exists(path)
            ? File.GetLastWriteTime(path)
            : DateTime.Now;
        UpdateDateText.Text = buildDate.ToString("yyyy-MM-dd");
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
