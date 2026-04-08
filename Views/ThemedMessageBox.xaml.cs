using System.Windows;
using System.Windows.Controls;
using EnvVar.Services;

namespace EnvVar.Views;

public partial class ThemedMessageBox : Window
{
    public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

    private ThemedMessageBox()
    {
        InitializeComponent();
        Loaded += (_, _) => ThemeService.UpdateTitleBar(this);
    }

    public static MessageBoxResult Show(
        Window owner,
        string message,
        string title,
        MessageBoxButton buttons = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None)
    {
        var dialog = new ThemedMessageBox
        {
            Owner = owner,
            Title = title
        };
        dialog.MessageText.Text = message;
        dialog.SetIcon(icon);
        dialog.AddButtons(buttons);
        dialog.ShowDialog();
        return dialog.Result;
    }

    private void SetIcon(MessageBoxImage icon)
    {
        IconText.Text = icon switch
        {
            MessageBoxImage.Warning => "⚠",
            MessageBoxImage.Error => "❌",
            MessageBoxImage.Question => "❓",
            MessageBoxImage.Information => "ℹ",
            _ => ""
        };

        if (string.IsNullOrEmpty(IconText.Text))
        {
            IconText.Visibility = Visibility.Collapsed;
        }
    }

    private void AddButtons(MessageBoxButton buttons)
    {
        switch (buttons)
        {
            case MessageBoxButton.OK:
                AddButton(LocalizationService.Get("Btn_OK"), MessageBoxResult.OK, true);
                break;
            case MessageBoxButton.OKCancel:
                AddButton(LocalizationService.Get("Btn_OK"), MessageBoxResult.OK, true);
                AddButton(LocalizationService.Get("Btn_Cancel"), MessageBoxResult.Cancel);
                break;
            case MessageBoxButton.YesNo:
                AddButton(LocalizationService.Get("Btn_Yes"), MessageBoxResult.Yes, true);
                AddButton(LocalizationService.Get("Btn_No"), MessageBoxResult.No);
                break;
            case MessageBoxButton.YesNoCancel:
                AddButton(LocalizationService.Get("Btn_Yes"), MessageBoxResult.Yes, true);
                AddButton(LocalizationService.Get("Btn_No"), MessageBoxResult.No);
                AddButton(LocalizationService.Get("Btn_Cancel"), MessageBoxResult.Cancel);
                break;
        }
    }

    private void AddButton(string text, MessageBoxResult result, bool isDefault = false)
    {
        var button = new Button
        {
            Content = text,
            Width = 80,
            Margin = new Thickness(8, 0, 0, 0),
            IsDefault = isDefault,
            IsCancel = result is MessageBoxResult.Cancel or MessageBoxResult.No
        };
        button.Click += (_, _) =>
        {
            Result = result;
            Close();
        };
        ButtonPanel.Children.Add(button);
    }
}
