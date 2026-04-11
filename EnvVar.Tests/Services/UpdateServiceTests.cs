using System;
using EnvVar.Services;
using Xunit;

namespace EnvVar.Tests.Services;

public class UpdateServiceTests
{
    [Fact]
    public void SelectLatestPublishedRelease_ShouldIgnoreDraftsAndPrereleases()
    {
        var releasesJson = """
            [
              {
                "tag_name": "v3.1415",
                "html_url": "https://example.com/v3.1415",
                "draft": true,
                "prerelease": false
              },
              {
                "tag_name": "v3.141.1",
                "html_url": "https://example.com/v3.141.1",
                "draft": false,
                "prerelease": false
              },
              {
                "tag_name": "v3.141.2-preview",
                "html_url": "https://example.com/v3.141.2-preview",
                "draft": false,
                "prerelease": true
              }
            ]
            """;

        var result = UpdateService.SelectLatestPublishedRelease(releasesJson);

        Assert.NotNull(result);
        Assert.Equal("v3.141.1", result!.TagName);
        Assert.Equal(new Version(3, 141, 1), result.Version);
    }

    [Fact]
    public void SelectLatestPublishedRelease_ShouldChooseHighestVersion_NotReleaseOrder()
    {
        var releasesJson = """
            [
              {
                "tag_name": "v3.141.9",
                "html_url": "https://example.com/v3.141.9",
                "draft": false,
                "prerelease": false
              },
              {
                "tag_name": "v3.1415",
                "html_url": "https://example.com/v3.1415",
                "draft": false,
                "prerelease": false
              },
              {
                "tag_name": "v3.141",
                "html_url": "https://example.com/v3.141",
                "draft": false,
                "prerelease": false
              }
            ]
            """;

        var result = UpdateService.SelectLatestPublishedRelease(releasesJson);

        Assert.NotNull(result);
        Assert.Equal("v3.1415", result!.TagName);
        Assert.Equal(new Version(3, 1415), result.Version);
    }
}
