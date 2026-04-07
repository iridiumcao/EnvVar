using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        var currentLang = LocalizationService.CurrentLanguage;
        LangEnUS.IsChecked = currentLang == "en-US";
        LangZhCN.IsChecked = currentLang == "zh-CN";
        LangZhTW.IsChecked = currentLang == "zh-TW";

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
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        TryRun(() => ViewModel.LoadVariables(), LocalizationService.Get("Msg_RefreshFailed"));
    }

    private void NewButton_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.StartCreateNew();
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.Editor.CommitStructuredChanges();

        if (ViewModel.WouldOverwriteExisting())
        {
            var overwrite = MessageBox.Show(
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
        var confirmation = MessageBox.Show(
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
                var confirm = MessageBox.Show(
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

    private void OpenMetadataDir_Click(object sender, RoutedEventArgs e)
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
            MessageBox.Show(this, ex.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PromptAdminRestart()
    {
        var result = MessageBox.Show(
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
