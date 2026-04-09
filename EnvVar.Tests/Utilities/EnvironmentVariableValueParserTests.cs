using EnvVar.Utilities;
using Xunit;

namespace EnvVar.Tests.Utilities;

public class EnvironmentVariableValueParserTests
{
    [Theory]
    [InlineData("C:\\path1;C:\\path2", true)]
    [InlineData("C:\\path1", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData(";", true)]
    public void IsMultiValue_ShouldDetectSemicolon(string? value, bool expected)
    {
        var result = EnvironmentVariableValueParser.IsMultiValue(value);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Split_ShouldSplitBySemicolonAndTrim()
    {
        var value = " C:\\path1 ; C:\\path2 ;; C:\\path3 ";
        var result = EnvironmentVariableValueParser.Split(value);

        Assert.Equal(3, result.Count);
        Assert.Equal("C:\\path1", result[0]);
        Assert.Equal("C:\\path2", result[1]);
        Assert.Equal("C:\\path3", result[2]);
    }

    [Fact]
    public void Split_ShouldReturnEmpty_WhenNotMultiValue()
    {
        var value = "C:\\path1";
        var result = EnvironmentVariableValueParser.Split(value);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("Short value", 20, "Short value")]
    [InlineData("This is a very long value that should be truncated for preview purposes", 20, "This is a very long ...")]
    [InlineData("Value with\r\nnewlines", 80, "Value with newlines")]
    public void BuildPreview_ShouldFormatCorrectly(string value, int maxLength, string expected)
    {
        var result = EnvironmentVariableValueParser.BuildPreview(value, maxLength);
        Assert.Equal(expected, result);
    }
}
