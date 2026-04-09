using System.IO;
using System.Text.Json;
using EnvVar.Models;
using EnvVar.Services;
using Xunit;

namespace EnvVar.Tests.Services;

public class MetadataStoreTests : IDisposable
{
    private readonly string _tempFile;

    public MetadataStoreTests()
    {
        _tempFile = Path.GetTempFileName();
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
        {
            File.Delete(_tempFile);
        }
    }

    [Fact]
    public void Load_ShouldReturnEmpty_WhenFileNotExists()
    {
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var store = new MetadataStore(nonExistentFile);

        var result = store.Load();

        Assert.Empty(result);
    }

    [Fact]
    public void SaveAndLoad_ShouldPreserveData()
    {
        var store = new MetadataStore(_tempFile);
        var metadata = new Dictionary<string, VariableMetadata>(StringComparer.OrdinalIgnoreCase)
        {
            ["VAR1@User"] = new VariableMetadata { Alias = "Alias1", Description = "Desc1" },
            ["VAR2@System"] = new VariableMetadata { Alias = "Alias2", Description = "Desc2" }
        };

        store.Save(metadata);
        var result = store.Load();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alias1", result["VAR1@User"].Alias);
        Assert.Equal("Desc2", result["VAR2@System"].Description);
    }

    [Fact]
    public void BuildKey_ShouldReturnCorrectFormat()
    {
        var key = MetadataStore.BuildKey("MY_VAR", EnvironmentVariableLevel.User);
        Assert.Equal("MY_VAR@User", key);
    }
}
