using System.Net.Http.Headers;
using System.Text.Json;

namespace VietIME.Core.Services;

/// <summary>
/// Service kiểm tra và tải bản cập nhật từ GitHub Releases.
/// Dùng chung cho cả 3 platform (Windows, macOS, Linux).
/// </summary>
public class UpdateService
{
    /// <summary>
    /// Version hiện tại của app — CẬP NHẬT KHI RELEASE.
    /// Phải khớp với Version trong .csproj.
    /// </summary>
    public const string AppVersion = "1.0.8";

    private const string GitHubApiUrl = "https://api.github.com/repos/donamvn/viet-ime/releases/latest";
    private const string GitHubReleasesUrl = "https://github.com/donamvn/viet-ime/releases/latest";

    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private UpdateInfo? _cachedInfo;
    private DateTime _cacheTime = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    static UpdateService()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("VietIME", AppVersion));
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
    }

    /// <summary>
    /// Kiểm tra phiên bản mới từ GitHub Releases.
    /// Cache kết quả 1 giờ. Force = true để bỏ qua cache.
    /// </summary>
    public async Task<UpdateInfo> CheckForUpdateAsync(bool force = false)
    {
        // Trả về cache nếu còn hiệu lực
        if (!force && _cachedInfo != null &&
            (DateTime.UtcNow - _cacheTime) < CacheDuration)
        {
            return _cachedInfo;
        }

        try
        {
            var response = await _httpClient.GetStringAsync(GitHubApiUrl);
            var json = JsonDocument.Parse(response);
            var root = json.RootElement;

            var tagName = root.GetProperty("tag_name").GetString() ?? "";
            var latestVersionStr = tagName.TrimStart('v');
            var body = root.GetProperty("body").GetString() ?? "";
            var htmlUrl = root.GetProperty("html_url").GetString() ?? GitHubReleasesUrl;

            // Parse assets để tìm download URL theo platform
            var assets = root.GetProperty("assets");
            string? dmgUrl = null;
            string? exeUrl = null;
            string? linuxUrl = null;

            foreach (var asset in assets.EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString() ?? "";
                var downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";

                if (name.EndsWith(".dmg", StringComparison.OrdinalIgnoreCase))
                    dmgUrl = downloadUrl;
                else if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    exeUrl = downloadUrl;
                else if (name.EndsWith(".AppImage.tar.gz", StringComparison.OrdinalIgnoreCase))
                    linuxUrl = downloadUrl;
            }

            // So sánh version
            bool hasUpdate = false;
            if (Version.TryParse(latestVersionStr, out var latestVersion) &&
                Version.TryParse(AppVersion, out var currentVersion))
            {
                hasUpdate = latestVersion > currentVersion;
            }

            _cachedInfo = new UpdateInfo
            {
                HasUpdate = hasUpdate,
                CurrentVersion = AppVersion,
                LatestVersion = latestVersionStr,
                ReleaseNotes = body,
                ReleaseUrl = htmlUrl,
                DmgDownloadUrl = dmgUrl,
                ExeDownloadUrl = exeUrl,
                LinuxDownloadUrl = linuxUrl
            };
            _cacheTime = DateTime.UtcNow;

            return _cachedInfo;
        }
        catch (Exception ex)
        {
            return new UpdateInfo
            {
                HasUpdate = false,
                CurrentVersion = AppVersion,
                LatestVersion = AppVersion,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Tải file từ URL vào đường dẫn chỉ định, báo cáo tiến trình qua callback.
    /// </summary>
    public async Task DownloadFileAsync(string url, string destPath,
        Action<long, long>? onProgress = null, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;

        await using var contentStream = await response.Content.ReadAsStreamAsync(ct);
        await using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[65536];
        long totalRead = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            totalRead += bytesRead;
            onProgress?.Invoke(totalRead, totalBytes);
        }
    }

    /// <summary>
    /// Xóa cache để lần check tiếp theo sẽ gọi API mới.
    /// </summary>
    public void InvalidateCache()
    {
        _cachedInfo = null;
        _cacheTime = DateTime.MinValue;
    }
}

/// <summary>
/// Kết quả kiểm tra cập nhật.
/// </summary>
public class UpdateInfo
{
    public bool HasUpdate { get; set; }
    public string CurrentVersion { get; set; } = "";
    public string LatestVersion { get; set; } = "";
    public string? ReleaseNotes { get; set; }
    public string? ReleaseUrl { get; set; }
    public string? Error { get; set; }

    // Platform-specific download URLs
    public string? DmgDownloadUrl { get; set; }
    public string? ExeDownloadUrl { get; set; }
    public string? LinuxDownloadUrl { get; set; }
}
