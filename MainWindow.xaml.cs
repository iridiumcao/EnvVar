using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using EnvVar.Services;
using EnvVar.Utilities;
using EnvVar.ViewModels;
using EnvVar.Views;
using Microsoft.Win32;

namespace EnvVar;

public partial class MainWindow : Window
{
    private string? _sortedPropertyName;
    private int _sortClickCount;
    private bool _isInternalSelectionChange;

    private static readonly Dictionary<string, string> ColumnResourceKeys = new()
    {
        ["NameDisplay"] = "Col_Name",
        ["Alias"] = "Col_Alias",
        ["Level"] = "Col_Level",
        ["Preview"] = "Col_Preview"
    };

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        Loaded += MainWindow_OnLoaded;
    }

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        ThemeService.UpdateTitleBar(this);

        var currentLang = LocalizationService.CurrentLanguage;
        LangEnUS.IsChecked = currentLang == "en-US";
        LangZhCN.IsChecked = currentLang == "zh-CN";
        LangZhTW.IsChecked = currentLang == "zh-TW";

        var currentTheme = ThemeService.CurrentTheme;
        ThemeAuto.IsChecked = currentTheme == "System";
        ThemeLight.IsChecked = currentTheme == "Light";
        ThemeDark.IsChecked = currentTheme == "Dark";

        TryRun(() => ViewModel.LoadVariables(), LocalizationService.Get("Msg_LoadFailed"));
        UpdateColumnHeaders();
        ViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.DisplayMode))
            {
                _sortedPropertyName = null;
                _sortClickCount = 0;
                UpdateColumnHeaders();
            }
        };

        if (VariableListView.View is GridView gridView)
        {
            foreach (var column in gridView.Columns)
            {
                ((INotifyPropertyChanged)column).PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == "ActualWidth")
                    {
                        Dispatcher.BeginInvoke(new Action(AdjustLastColumnWidth), System.Windows.Threading.DispatcherPriority.Loaded);
                    }
                };
            }
        }
    }

    private bool _isAdjustingColumn;

    private void AdjustLastColumnWidth()
    {
        if (_isAdjustingColumn) return;

        if (VariableListView.View is GridView gridView && gridView.Columns.Count > 0)
        {
            var scrollViewer = GetScrollViewer(VariableListView);
            if (scrollViewer != null)
            {
                double totalWidth = 0;
                for (int i = 0; i < gridView.Columns.Count - 1; i++)
                {
                    totalWidth += gridView.Columns[i].ActualWidth;
                }

                // Subtract 16 pixels to account for ListViewItem's horizontal padding (4+4) 
                // and a safe margin, ensuring the horizontal scrollbar never stays visible
                // when it's not needed.
                double remainingWidth = scrollViewer.ViewportWidth - totalWidth - 16;
                if (remainingWidth >= 50)
                {
                    _isAdjustingColumn = true;
                    gridView.Columns[^1].Width = remainingWidth;
                    _isAdjustingColumn = false;
                }
            }
        }
    }

    private void VariableListView_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(AdjustLastColumnWidth), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!CheckUnsavedChanges()) return;
        TryRun(() => ViewModel.LoadVariables(), LocalizationService.Get("Msg_RefreshFailed"));
    }

    private void NewButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!CheckUnsavedChanges()) return;
        ViewModel.StartCreateNew();
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.Editor.CommitStructuredChanges();

        if (ViewModel.WouldOverwriteExisting())
        {
            var overwrite = ThemedMessageBox.Show(
                this,
                LocalizationService.Get("Msg_ConfirmOverwrite"),
                LocalizationService.Get("Msg_ConfirmOverwriteTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (overwrite != MessageBoxResult.Yes)
            {
                return;
            }
        }

        TryRun(() => ViewModel.SaveCurrent(), LocalizationService.Get("Msg_SaveFailed"));
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        var confirmation = ThemedMessageBox.Show(
            this,
            LocalizationService.Get("Msg_ConfirmDelete"),
            LocalizationService.Get("Msg_ConfirmDeleteTitle"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        TryRun(() => ViewModel.DeleteCurrent(), LocalizationService.Get("Msg_DeleteFailed"));
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.CancelEditing();
    }

    private void VariableListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInternalSelectionChange) return;

        if (e.RemovedItems.Count > 0 && ViewModel.Editor.HasChanges)
        {
            _isInternalSelectionChange = true;
            var oldItem = e.RemovedItems[0];
            var newItem = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;

            // Revert selection temporarily to check
            VariableListView.SelectedItem = oldItem;

            if (CheckUnsavedChanges())
            {
                VariableListView.SelectedItem = newItem;
            }
            _isInternalSelectionChange = false;
        }
    }

    private static ScrollViewer? GetScrollViewer(DependencyObject depObj)
    {
        if (depObj is ScrollViewer scrollViewer) return scrollViewer;

        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
            var result = GetScrollViewer(child);
            if (result != null) return result;
        }
        return null;
    }

    private void MainWindow_OnClosing(object sender, CancelEventArgs e)
    {
        if (!CheckUnsavedChanges())
        {
            e.Cancel = true;
        }
    }

    private bool CheckUnsavedChanges()
    {
        if (!ViewModel.Editor.HasChanges) return true;

        var result = ThemedMessageBox.Show(
            this,
            LocalizationService.Get("Msg_UnsavedChanges"),
            LocalizationService.Get("Msg_UnsavedChangesTitle"),
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            ViewModel.Editor.CommitStructuredChanges();
            if (ViewModel.WouldOverwriteExisting())
            {
                var overwrite = ThemedMessageBox.Show(
                    this,
                    LocalizationService.Get("Msg_ConfirmOverwrite"),
                    LocalizationService.Get("Msg_ConfirmOverwriteTitle"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (overwrite != MessageBoxResult.Yes) return false;
            }

            TryRun(() => ViewModel.SaveCurrent(), LocalizationService.Get("Msg_SaveFailed"));
            return true;
        }

        if (result == MessageBoxResult.No)
        {
            return true;
        }

        return false;
    }

    private void ExportMenu_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = LocalizationService.Get("Msg_ExportTitle"),
            Filter = LocalizationService.Get("Msg_JsonFilter"),
            FileName = $"envvar-export-{DateTime.Now:yyyyMMdd}.json"
        };

        if (dialog.ShowDialog(this) == true)
        {
            TryRun(() =>
            {
                ViewModel.Export(dialog.FileName);
                ViewModel.StatusMessage = LocalizationService.Get("Msg_ExportSuccess", dialog.FileName);
            }, LocalizationService.Get("Msg_ExportTitle"));
        }
    }

    private void ImportMenu_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = LocalizationService.Get("Msg_ImportTitle"),
            Filter = LocalizationService.Get("Msg_JsonFilter")
        };

        if (dialog.ShowDialog(this) == true)
        {
            TryRun(() =>
            {
                var variables = ViewModel.LoadImportFile(dialog.FileName);
                var confirm = ThemedMessageBox.Show(
                    this,
                    LocalizationService.Get("Msg_ImportConfirm", variables.Count),
                    LocalizationService.Get("Msg_ImportConfirmTitle"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                {
                    var count = ViewModel.Import(variables);
                    ViewModel.LoadVariables();
                    ViewModel.StatusMessage = LocalizationService.Get("Msg_ImportSuccess", count);
                }
            }, LocalizationService.Get("Msg_ImportTitle"));
        }
    }

    private void OpenDataDir_Click(object sender, RoutedEventArgs e)
    {
        var dir = System.IO.Path.GetDirectoryName(ViewModel.MetadataPath);
        if (!string.IsNullOrWhiteSpace(dir) && System.IO.Directory.Exists(dir))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = dir,
                UseShellExecute = true
            });
        }
    }

    private void ExitMenu_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SettingsMenu_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow { Owner = this };
        settingsWindow.ShowDialog();
    }

    private void LanguageMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is string cultureCode)
        {
            LocalizationService.SwitchLanguage(cultureCode);

            LangZhCN.IsChecked = cultureCode == "zh-CN";
            LangZhTW.IsChecked = cultureCode == "zh-TW";
            LangEnUS.IsChecked = cultureCode == "en-US";
            UpdateColumnHeaders();
            ViewModel.Editor.NotifyHeaderChanged();
        }
    }

    private void ThemeMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is string themeName)
        {
            ThemeService.ApplyTheme(themeName);

            ThemeAuto.IsChecked = themeName == "System";
            ThemeLight.IsChecked = themeName == "Light";
            ThemeDark.IsChecked = themeName == "Dark";
        }
    }

    private async void CheckUpdateMenu_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.StatusMessage = LocalizationService.Get("Msg_UpdateAvailableTitle"); // Using as a temporary status, or we can just say "Checking..."
        var originalCursor = Mouse.OverrideCursor;
        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

        try
        {
            var updateInfo = await UpdateService.CheckForUpdatesAsync();
            
            // Reset cursor BEFORE showing any blocking dialog
            Mouse.OverrideCursor = originalCursor;
            ViewModel.StatusMessage = string.Empty;

            if (updateInfo.HasUpdate)
            {
                var result = ThemedMessageBox.Show(
                    this,
                    LocalizationService.Get("Msg_UpdateAvailable", updateInfo.LatestVersion),
                    LocalizationService.Get("Msg_UpdateAvailableTitle"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(updateInfo.ReleaseUrl))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = updateInfo.ReleaseUrl,
                        UseShellExecute = true
                    });
                }
            }
            else
            {
                // Just use the latest version if we successfully parsed, else generic
                ThemedMessageBox.Show(
                    this,
                    LocalizationService.Get("Msg_UpdateNotNeeded"),
                    LocalizationService.Get("Msg_UpdateNotNeededTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception)
        {
            Mouse.OverrideCursor = originalCursor;
            ViewModel.StatusMessage = string.Empty;

            ThemedMessageBox.Show(
                this,
                LocalizationService.Get("Msg_UpdateCheckFailed"),
                LocalizationService.Get("Msg_UpdateAvailableTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void StarOnGitHubMenu_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/iridiumcao/EnvVar",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ThemedMessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ReportIssueMenu_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/iridiumcao/EnvVar/issues",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ThemedMessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AboutMenu_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow { Owner = this };
        aboutWindow.ShowDialog();
    }

    private void ColumnHeader_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader header || header.Column == null)
        {
            return;
        }

        if (header.Column.DisplayMemberBinding is not Binding binding)
        {
            return;
        }

        var sortBy = binding.Path.Path;
        if (string.IsNullOrEmpty(sortBy))
        {
            return;
        }

        if (sortBy == _sortedPropertyName)
        {
            _sortClickCount = (_sortClickCount + 1) % 3;
        }
        else
        {
            _sortedPropertyName = sortBy;
            _sortClickCount = 1;
        }

        if (_sortClickCount == 0)
        {
            _sortedPropertyName = null;
            ViewModel.ResetColumnSort();
        }
        else
        {
            var direction = _sortClickCount == 1
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;
            ViewModel.ApplyColumnSort(sortBy, direction);
        }

        UpdateColumnHeaders();
    }

    private void UpdateColumnHeaders()
    {
        foreach (var column in VariableGridView.Columns)
        {
            if (column.DisplayMemberBinding is Binding binding &&
                ColumnResourceKeys.TryGetValue(binding.Path.Path, out var resourceKey))
            {
                var text = LocalizationService.Get(resourceKey);
                if (binding.Path.Path == _sortedPropertyName && _sortClickCount > 0)
                {
                    text += _sortClickCount == 1 ? " \u25B2" : " \u25BC";
                }

                column.Header = text;
            }
        }
    }

    private void AddValueItem_Click(object sender, RoutedEventArgs e)
    {
        var index = EditableValuesList.SelectedIndex;
        ViewModel.Editor.AddValueItem(index >= 0 ? index + 1 : -1);
    }

    private void RemoveValueItem_Click(object sender, RoutedEventArgs e)
    {
        var index = EditableValuesList.SelectedIndex;
        if (index >= 0)
        {
            ViewModel.Editor.RemoveValueItemAt(index);
        }
    }

    private void MoveValueItemUp_Click(object sender, RoutedEventArgs e)
    {
        var index = EditableValuesList.SelectedIndex;
        ViewModel.Editor.MoveValueItemUp(index);
        if (index > 0)
        {
            EditableValuesList.SelectedIndex = index - 1;
        }
    }

    private void MoveValueItemDown_Click(object sender, RoutedEventArgs e)
    {
        var index = EditableValuesList.SelectedIndex;
        ViewModel.Editor.MoveValueItemDown(index);
        if (index >= 0 && index < EditableValuesList.Items.Count - 1)
        {
            EditableValuesList.SelectedIndex = index + 1;
        }
    }

    private void SortValuesAsc_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Editor.SortValuesAscending();
    }

    private void SortValuesDesc_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Editor.SortValuesDescending();
    }

    private void HistoryButton_Click(object sender, RoutedEventArgs e)
    {
        var history = ViewModel.GetCurrentVariableHistory();
        var menu = new ContextMenu();

        if (history.Count == 0)
        {
            menu.Items.Add(new MenuItem
            {
                Header = LocalizationService.Get("Msg_NoHistory"),
                IsEnabled = false
            });
        }
        else
        {
            foreach (var entry in history)
            {
                var preview = EnvironmentVariableValueParser.BuildPreview(entry.Value, 60);
                var item = new MenuItem
                {
                    Header = $"{entry.Timestamp:g}  {preview}",
                    Tag = entry
                };
                item.Click += HistoryEntry_Click;
                menu.Items.Add(item);
            }
        }

        menu.PlacementTarget = HistoryButton;
        menu.IsOpen = true;
    }

    private void HistoryEntry_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item && item.Tag is VariableHistoryEntry entry)
        {
            ViewModel.RestoreFromHistory(entry);
        }
    }

    private void TryRun(Action action, string title)
    {
        try
        {
            action();
        }
        catch (SecurityException)
        {
            PromptAdminRestart();
        }
        catch (UnauthorizedAccessException)
        {
            PromptAdminRestart();
        }
        catch (Exception ex)
        {
            ViewModel.StatusMessage = ex.Message;
            ThemedMessageBox.Show(this, ex.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PromptAdminRestart()
    {
        var result = ThemedMessageBox.Show(
            this,
            LocalizationService.Get("Msg_AdminRequired"),
            LocalizationService.Get("Msg_AdminRequiredTitle"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath!,
                    UseShellExecute = true,
                    Verb = "runas"
                });
                Application.Current.Shutdown();
            }
            catch (Exception)
            {
                // User cancelled the UAC prompt
            }
        }
    }
}
