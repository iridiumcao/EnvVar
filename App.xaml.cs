using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
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
        RegisterGlobalExceptionHandlers();

        ThemeService.Initialize();
        LoggingService.Shared.EnsureLogDirectoryExists();

        var savedLanguage = LocalizationService.LoadSavedLanguage();
        if (savedLanguage != "en-US")
        {
            LocalizationService.SwitchLanguage(savedLanguage);
        }

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
        LoggingService.Shared.Information("Application started.", action: "Application Started");

        if (_activateWhenReady)
        {
            ActivateMainWindow();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        LoggingService.Shared.Information("Application exiting.", action: "Application Exiting");
        UnregisterGlobalExceptionHandlers();

        ReleaseSingleInstanceMutex();

        base.OnExit(e);
    }

    public void ReleaseSingleInstanceMutex()
    {
        if (_singleInstanceService is not null)
        {
            _singleInstanceService.ActivationRequested -= OnActivationRequested;
            _singleInstanceService.Dispose();
            _singleInstanceService = null;
        }
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

    private void RegisterGlobalExceptionHandlers()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;
    }

    private void UnregisterGlobalExceptionHandlers()
    {
        DispatcherUnhandledException -= OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnTaskSchedulerUnobservedTaskException;
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LoggingService.Shared.Error(
            "Unhandled exception on the UI thread.",
            action: "Unhandled UI Exception",
            exception: e.Exception);
    }

    private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LoggingService.Shared.Error(
            "Unhandled exception on a non-UI thread.",
            action: "Unhandled Non-UI Exception",
            context: new Dictionary<string, string?>
            {
                ["IsTerminating"] = e.IsTerminating.ToString()
            },
            exception: e.ExceptionObject as Exception);
    }

    private static void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LoggingService.Shared.Error(
            "Unobserved task exception.",
            action: "Unobserved Task Exception",
            exception: e.Exception);
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}
