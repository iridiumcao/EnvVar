using System.Windows;
using EnvVar.ViewModels;

namespace EnvVar;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        Loaded += MainWindow_OnLoaded;
    }

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        TryRun(() => ViewModel.LoadVariables(), "加载环境变量失败");
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        TryRun(() => ViewModel.LoadVariables(), "刷新环境变量失败");
    }

    private void NewButton_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.StartCreateNew();
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.WouldOverwriteExisting())
        {
            var overwrite = MessageBox.Show(
                this,
                "目标名称和级别下已存在同名环境变量，是否覆盖？",
                "确认覆盖",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (overwrite != MessageBoxResult.Yes)
            {
                return;
            }
        }

        TryRun(() => ViewModel.SaveCurrent(), "保存环境变量失败");
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        var confirmation = MessageBox.Show(
            this,
            "确认删除当前环境变量？",
            "确认删除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        TryRun(() => ViewModel.DeleteCurrent(), "删除环境变量失败");
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.CancelEditing();
    }

    private void TryRun(Action action, string title)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            ViewModel.StatusMessage = ex.Message;
            MessageBox.Show(this, ex.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
