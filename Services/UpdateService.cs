using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EnvVar.Services;

public class UpdateInfo
{
    public bool HasUpdate { get; set; }
    public string LatestVersion { get; set; } = string.Empty;
    public string ReleaseUrl { get; set; } = string.Empty;
}

public static class UpdateService
{
    private const string GitHubApiUrl = "https://api.github.com/repos/iridiumcao/EnvVar/releases";
    private static readonly Regex VersionPattern = new(@"\d+(\.\d+){1,3}", RegexOptions.Compiled);
    private static readonly HttpClient _httpClient;

    static UpdateService()
    {
        _httpClient = new HttpClient();
        // GitHub API requires a User-Agent header
        var version = GetCurrentVersion().ToString();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EnvVar", version));
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public static Version GetCurrentVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);
    }

    internal static bool TryParseVersion(string? input, out Version version)
    {
        version = new Version(0, 0);

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var match = VersionPattern.Match(input);
        if (!match.Success)
        {
            return false;
        }

        if (!Version.TryParse(match.Value, out var parsedVersion))
        {
            return false;
        }

        version = parsedVersion;
        return true;
    }

    internal static ReleaseCandidate? SelectLatestPublishedRelease(string releasesJson)
    {
        using var document = JsonDocument.Parse(releasesJson);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        ReleaseCandidate? latest = null;

        foreach (var release in document.RootElement.EnumerateArray())
        {
            if (release.TryGetProperty("draft", out var draftElement) && draftElement.GetBoolean())
            {
                continue;
            }

            if (release.TryGetProperty("prerelease", out var prereleaseElement) && prereleaseElement.GetBoolean())
            {
                continue;
            }

            if (!release.TryGetProperty("tag_name", out var tagElement))
            {
                continue;
            }

            var tagName = tagElement.GetString();
            if (!TryParseVersion(tagName, out var version))
            {
                continue;
            }

            var releaseUrl = release.TryGetProperty("html_url", out var urlElement)
                ? urlElement.GetString() ?? string.Empty
                : string.Empty;

            var candidate = new ReleaseCandidate(tagName ?? string.Empty, version, releaseUrl);
            if (latest is null || NormalizeVersion(candidate.Version) > NormalizeVersion(latest.Version))
            {
                latest = candidate;
            }
        }

        return latest;
    }

    private static Version NormalizeVersion(Version v)
    {
        return new Version(
            Math.Max(0, v.Major),
            Math.Max(0, v.Minor),
            Math.Max(0, v.Build),
            Math.Max(0, v.Revision)
        );
    }

    public static async Task<UpdateInfo> CheckForUpdatesAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(GitHubApiUrl);

            var latestRelease = SelectLatestPublishedRelease(response);
            if (latestRelease is not null)
            {
                var currentVersion = GetCurrentVersion();
                return new UpdateInfo
                {
                    HasUpdate = NormalizeVersion(latestRelease.Version) > NormalizeVersion(currentVersion),
                    LatestVersion = latestRelease.TagName,
                    ReleaseUrl = latestRelease.ReleaseUrl
                };
            }
        }
        catch (Exception)
        {
            // Ignore network or parsing errors, just return no update available
        }

        return new UpdateInfo { HasUpdate = false };
    }

    internal sealed record ReleaseCandidate(string TagName, Version Version, string ReleaseUrl);
}
