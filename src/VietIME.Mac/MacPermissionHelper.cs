namespace VietIME.Mac;

/// <summary>
/// Kiểm tra và yêu cầu quyền Accessibility / Input Monitoring trên macOS.
/// 
/// macOS yêu cầu app phải có quyền Input Monitoring để dùng CGEventTap.
/// - CGPreflightListenEventAccess(): kiểm tra quyền mà KHÔNG hiện dialog
/// - CGRequestListenEventAccess(): hiện dialog hệ thống yêu cầu quyền
/// 
/// LƯU Ý: App phải restart sau khi user cấp quyền lần đầu.
/// App được code-sign sẽ giữ quyền ổn định qua các lần update.
/// </summary>
public static class MacPermissionHelper
{
    /// <summary>
    /// Kiểm tra app đã có quyền Input Monitoring chưa.
    /// Không hiện dialog - an toàn để gọi bất cứ lúc nào.
    /// </summary>
    public static bool HasInputMonitoringPermission()
    {
        try
        {
            return MacNativeMethods.CGPreflightListenEventAccess();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Yêu cầu quyền Input Monitoring.
    /// Hiện dialog hệ thống nếu chưa có quyền.
    /// Return true nếu đã có quyền (hoặc vừa được cấp).
    /// </summary>
    public static bool RequestInputMonitoringPermission()
    {
        try
        {
            return MacNativeMethods.CGRequestListenEventAccess();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Kiểm tra app đã có quyền post event chưa.
    /// </summary>
    public static bool HasPostEventPermission()
    {
        try
        {
            return MacNativeMethods.CGPreflightPostEventAccess();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Yêu cầu quyền post event.
    /// </summary>
    public static bool RequestPostEventPermission()
    {
        try
        {
            return MacNativeMethods.CGRequestPostEventAccess();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Kiểm tra tất cả quyền cần thiết. 
    /// Return thông báo lỗi nếu thiếu quyền, null nếu đủ.
    /// </summary>
    public static string? CheckAllPermissions()
    {
        bool hasListen = HasInputMonitoringPermission();
        bool hasPost = HasPostEventPermission();

        if (!hasListen && !hasPost)
        {
            return "VietIME cần 2 quyền để hoạt động:\n" +
                   "1. System Settings → Privacy & Security → Input Monitoring → bật VietIME/Warp\n" +
                   "2. System Settings → Privacy & Security → Accessibility → bật VietIME/Warp\n\n" +
                   "Sau khi cấp quyền, cần khởi động lại VietIME.";
        }

        if (!hasListen)
        {
            return "VietIME cần quyền Input Monitoring.\n" +
                   "System Settings → Privacy & Security → Input Monitoring → bật VietIME/Warp";
        }

        if (!hasPost)
        {
            return "VietIME cần quyền Accessibility để gửi phím.\n" +
                   "System Settings → Privacy & Security → Accessibility → bật VietIME/Warp";
        }

        return null;
    }

    /// <summary>
    /// Yêu cầu tất cả quyền cần thiết.
    /// Hiện dialog hệ thống cho mỗi quyền chưa có.
    /// Return true nếu đã đủ tất cả quyền.
    /// </summary>
    public static bool RequestAllPermissions()
    {
        bool listen = RequestInputMonitoringPermission();
        bool post = RequestPostEventPermission();
        return listen && post;
    }
}
