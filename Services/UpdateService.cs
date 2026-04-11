using System;
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
    private const string GitHubApiUrl = "https://api.github.com/repos/iridiumcao/EnvVar/releases/latest";
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
            using var document = JsonDocument.Parse(response);
            var root = document.RootElement;
            
            if (root.TryGetProperty("tag_name", out var tagElement) && root.TryGetProperty("html_url", out var urlElement))
            {
                var tagName = tagElement.GetString() ?? string.Empty;
                var releaseUrl = urlElement.GetString() ?? string.Empty;

                // Extract version number from tag (e.g., "v1.0.1" -> "1.0.1", "v3.141" -> "3.141")
                var match = Regex.Match(tagName, @"\d+(\.\d+){1,3}");
                if (match.Success && Version.TryParse(match.Value, out var latestVersion))
                {
                    var currentVersion = GetCurrentVersion();
                    return new UpdateInfo
                    {
                        HasUpdate = NormalizeVersion(latestVersion) > NormalizeVersion(currentVersion),
                        LatestVersion = tagName,
                        ReleaseUrl = releaseUrl
                    };
                }
            }
        }
        catch (Exception)
        {
            // Ignore network or parsing errors, just return no update available
        }

        return new UpdateInfo { HasUpdate = false };
    }
}