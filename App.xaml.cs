using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using EnvVar.Services;

namespace EnvVar;

public partial class App : Application
{
    private const int SwRestore = 9;
    private SingleInstanceService? _singleInstanceService;
    private bool _activateWhenReady;

    protected override void OnStartup(StartupEventArgs e)
    {
        var singleInstanceService = new SingleInstanceService();
        if (!singleInstanceService.TryAcquirePrimaryInstance())
        {
            singleInstanceService.SignalPrimaryInstance();
            singleInstanceService.Dispose();
            Shutdown();
            return;
        }

        _singleInstanceService = singleInstanceService;
        _singleInstanceService.ActivationRequested += OnActivationRequested;

        base.OnStartup(e);

        ThemeService.Initialize();

        var savedLanguage = LocalizationService.LoadSavedLanguage();
        if (savedLanguage != "en-US")
        {
            LocalizationService.SwitchLanguage(savedLanguage);
        }

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();

        if (_activateWhenReady)
        {
            ActivateMainWindow();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_singleInstanceService is not null)
        {
            _singleInstanceService.ActivationRequested -= OnActivationRequested;
            _singleInstanceService.Dispose();
        }

        base.OnExit(e);
    }

    private void OnActivationRequested(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            if (MainWindow is null)
            {
                _activateWhenReady = true;
                return;
            }

            ActivateMainWindow();
        });
    }

    private void ActivateMainWindow()
    {
        if (MainWindow is not Window mainWindow)
        {
            return;
        }

        _activateWhenReady = false;

        if (!mainWindow.IsVisible)
        {
            mainWindow.Show();
        }

        if (mainWindow.WindowState == WindowState.Minimized)
        {
            mainWindow.WindowState = WindowState.Normal;
        }

        var handle = new WindowInteropHelper(mainWindow).Handle;
        if (handle != IntPtr.Zero)
        {
            _ = ShowWindow(handle, SwRestore);
            _ = SetForegroundWindow(handle);
        }

        mainWindow.Topmost = true;
        mainWindow.Topmost = false;
        mainWindow.Activate();
        _ = mainWindow.Focus();
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}
