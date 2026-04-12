using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using EnvVar.Models;

namespace EnvVar.Services;

public sealed class LoggingService
{
    private static readonly Encoding FileEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly string DefaultLogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "EnvVar",
        "Logs");

    private readonly object _syncRoot = new();
    private readonly Func<DateTime> _nowProvider;
    private readonly Func<AppSettings> _settingsProvider;

    public LoggingService(
        string? logDirectory = null,
        Func<DateTime>? nowProvider = null,
        Func<AppSettings>? settingsProvider = null)
    {
        LogDirectory = logDirectory ?? DefaultLogDirectory;
        _nowProvider = nowProvider ?? (() => DateTime.Now);
        _settingsProvider = settingsProvider ?? (() => SettingsService.Current);
    }

    public static LoggingService Shared { get; } = new();

    public string LogDirectory { get; }

    public void EnsureLogDirectoryExists()
    {
        lock (_syncRoot)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnvVar logging directory creation failed: {ex}");
            }
        }
    }

    public void Debug(string message, string? action = null, IReadOnlyDictionary<string, string?>? context = null, Exception? exception = null)
    {
        Log(AppLogLevel.Debug, message, action, context, exception);
    }

    public void Information(string message, string? action = null, IReadOnlyDictionary<string, string?>? context = null, Exception? exception = null)
    {
        Log(AppLogLevel.Information, message, action, context, exception);
    }

    public void Warning(string message, string? action = null, IReadOnlyDictionary<string, string?>? context = null, Exception? exception = null)
    {
        Log(AppLogLevel.Warning, message, action, context, exception);
    }

    public void Error(string message, string? action = null, IReadOnlyDictionary<string, string?>? context = null, Exception? exception = null)
    {
        Log(AppLogLevel.Error, message, action, context, exception);
    }

    public void Log(
        AppLogLevel level,
        string message,
        string? action = null,
        IReadOnlyDictionary<string, string?>? context = null,
        Exception? exception = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var settings = _settingsProvider();
        if (level < settings.LogLevel)
        {
            return;
        }

        var timestamp = _nowProvider();
        var entry = BuildEntry(timestamp, level, message.Trim(), action, context, exception);

        lock (_syncRoot)
        {
            try
            {
                EnsureLogDirectoryExists();
                CleanupExpiredLogs(timestamp.Date, settings.LogRetentionDays);
                var filePath = GetTargetFilePath(timestamp.Date, entry, settings.MaxLogFileSizeMb);
                File.AppendAllText(filePath, entry, FileEncoding);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnvVar logging failed: {ex}");
            }
        }
    }

    private string GetTargetFilePath(DateTime date, string entry, int maxFileSizeMb)
    {
        var entryBytes = FileEncoding.GetByteCount(entry);
        var maxBytes = Math.Max(1L, maxFileSizeMb) * 1024 * 1024;

        for (var sequence = 0; ; sequence++)
        {
            var path = Path.Combine(LogDirectory, BuildLogFileName(date, sequence));
            if (!File.Exists(path))
            {
                return path;
            }

            var length = new FileInfo(path).Length;
            if (length + entryBytes <= maxBytes)
            {
                return path;
            }
        }
    }

    private void CleanupExpiredLogs(DateTime today, int retentionDays)
    {
        if (retentionDays <= 0 || !Directory.Exists(LogDirectory))
        {
            return;
        }

        var cutoffDate = today.AddDays(-(retentionDays - 1));
        foreach (var file in Directory.EnumerateFiles(LogDirectory, "*.log"))
        {
            if (TryParseLogDate(file, out var logDate) && logDate < cutoffDate)
            {
                File.Delete(file);
            }
        }
    }

    private static string BuildEntry(
        DateTime timestamp,
        AppLogLevel level,
        string message,
        string? action,
        IReadOnlyDictionary<string, string?>? context,
        Exception? exception)
    {
        var builder = new StringBuilder();
        builder.Append("Time: ").AppendLine(timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        builder.Append("Level: ").AppendLine(level.ToString());

        if (!string.IsNullOrWhiteSpace(action))
        {
            builder.Append("Action: ").AppendLine(action);
        }

        if (context is not null)
        {
            foreach (var pair in context)
            {
                if (string.IsNullOrWhiteSpace(pair.Value))
                {
                    continue;
                }

                builder.Append(pair.Key).Append(": ").AppendLine(pair.Value);
            }
        }

        builder.Append("Message: ").AppendLine(message);

        if (exception is not null)
        {
            builder.Append("ExceptionType: ").AppendLine(exception.GetType().FullName ?? exception.GetType().Name);
            builder.AppendLine("Exception:");
            builder.AppendLine(exception.ToString());
        }

        builder.AppendLine();
        return builder.ToString();
    }

    private static string BuildLogFileName(DateTime date, int sequence)
    {
        return sequence == 0
            ? $"{date:yyyy-MM-dd}.log"
            : $"{date:yyyy-MM-dd}.{sequence}.log";
    }

    internal static bool TryParseLogDate(string path, out DateTime date)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        if (fileName.Length >= 10 &&
            DateTime.TryParseExact(
                fileName[..10],
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
        {
            return true;
        }

        date = default;
        return false;
    }
}
