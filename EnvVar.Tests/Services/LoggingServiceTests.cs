using System.IO;
using EnvVar.Models;
using EnvVar.Services;
using Xunit;

namespace EnvVar.Tests.Services;

public sealed class LoggingServiceTests : IDisposable
{
    private readonly string _logDirectory;

    public LoggingServiceTests()
    {
        _logDirectory = Path.Combine(Path.GetTempPath(), "EnvVar.Tests", Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_logDirectory))
        {
            Directory.Delete(_logDirectory, recursive: true);
        }
    }

    [Fact]
    public void Log_ShouldWriteReadableEntry_WithContext()
    {
        var timestamp = new DateTime(2026, 4, 12, 10, 23, 11);
        var service = CreateService(timestamp, new AppSettings());

        service.Error(
            "Access denied",
            action: "Save Environment Variable",
            context: new Dictionary<string, string?>
            {
                ["Variable"] = "JAVA_HOME",
                ["Value"] = @"C:\Java\jdk17"
            });

        var filePath = Path.Combine(_logDirectory, "2026-04-12.log");
        var content = File.ReadAllText(filePath);

        Assert.Contains("Time: 2026-04-12 10:23:11", content);
        Assert.Contains("Level: Error", content);
        Assert.Contains("Action: Save Environment Variable", content);
        Assert.Contains("Variable: JAVA_HOME", content);
        Assert.Contains(@"Value: C:\Java\jdk17", content);
        Assert.Contains("Message: Access denied", content);
    }

    [Fact]
    public void Log_ShouldRespectConfiguredLevel()
    {
        var service = CreateService(
            new DateTime(2026, 4, 12, 10, 23, 11),
            new AppSettings { LogLevel = AppLogLevel.Warning });

        service.Information("This should be skipped.");
        service.Warning("This should be written.");

        var filePath = Path.Combine(_logDirectory, "2026-04-12.log");
        var content = File.ReadAllText(filePath);

        Assert.DoesNotContain("This should be skipped.", content);
        Assert.Contains("This should be written.", content);
    }

    [Fact]
    public void Log_ShouldRollToNextFile_WhenMaxSizeExceeded()
    {
        var settings = new AppSettings { MaxLogFileSizeMb = 1 };
        var service = CreateService(new DateTime(2026, 4, 12, 10, 23, 11), settings);
        var firstFile = Path.Combine(_logDirectory, "2026-04-12.log");

        Directory.CreateDirectory(_logDirectory);
        File.WriteAllText(firstFile, new string('A', 1024 * 1024));

        service.Information("rolled");

        Assert.True(File.Exists(firstFile));
        Assert.True(File.Exists(Path.Combine(_logDirectory, "2026-04-12.1.log")));
    }

    [Fact]
    public void Log_ShouldDeleteExpiredFiles_BasedOnRetention()
    {
        var today = new DateTime(2026, 4, 12, 10, 23, 11);
        var settings = new AppSettings { LogRetentionDays = 14 };
        var service = CreateService(today, settings);

        Directory.CreateDirectory(_logDirectory);
        File.WriteAllText(Path.Combine(_logDirectory, "2026-03-28.log"), "old");
        File.WriteAllText(Path.Combine(_logDirectory, "2026-04-11.log"), "recent");

        service.Information("cleanup");

        Assert.False(File.Exists(Path.Combine(_logDirectory, "2026-03-28.log")));
        Assert.True(File.Exists(Path.Combine(_logDirectory, "2026-04-11.log")));
    }

    [Fact]
    public void EnsureLogDirectoryExists_ShouldCreateDirectory()
    {
        var service = CreateService(new DateTime(2026, 4, 12, 10, 23, 11), new AppSettings());

        service.EnsureLogDirectoryExists();

        Assert.True(Directory.Exists(_logDirectory));
    }

    private LoggingService CreateService(DateTime timestamp, AppSettings settings)
    {
        return new LoggingService(_logDirectory, () => timestamp, () => settings);
    }
}
