using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using VietIME.Core.Engines;
using VietIME.Core.Services;

namespace VietIME.Mac.App;

public partial class App : Application
{
    private MacKeyboardHook? _hook;
    private MainWindow? _settingsWindow;

    // Commands cho NativeMenu bindings
    public ICommand ToggleCommand { get; }
    public ICommand SetTelexCommand { get; }
    public ICommand SetVniCommand { get; }
    public ICommand ShowSettingsCommand { get; }
    public ICommand ExitCommand { get; }

    public bool NotificationsEnabled { get; set; } = false;

    public MacKeyboardHook? Hook => _hook;

    public App()
    {
        ToggleCommand = new RelayCommand(ToggleIME);
        SetTelexCommand = new RelayCommand(() => SetEngine("Telex"));
        SetVniCommand = new RelayCommand(() => SetEngine("VNI"));
        ShowSettingsCommand = new RelayCommand(ShowSettings);
        ExitCommand = new RelayCommand(ExitApp);

        DataContext = this;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Không hiện MainWindow khi khởi động - chỉ chạy trong tray
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        // Khởi tạo keyboard hook
        Console.WriteLine("[VietIME] Khởi tạo MacKeyboardHook...");
        _hook = new MacKeyboardHook();
        _hook.Engine = new TelexEngine();
        _hook.EnabledChanged += Hook_EnabledChanged;
        _hook.Error += Hook_Error;
        _hook.DebugLog += msg => Console.WriteLine($"[VietIME] {msg}");
        _hook.Install();
        Console.WriteLine("[VietIME] App đã khởi động. Tray icon trên menu bar.");

        // Nếu thiếu quyền → mở Settings window ngay để hiện hướng dẫn cấp quyền
        if (!MacPermissionHelper.HasInputMonitoringPermission() ||
            !MacPermissionHelper.HasPostEventPermission())
        {
            Console.WriteLine("[VietIME] Thiếu quyền - hiện hướng dẫn cấp quyền...");
            ShowSettings();
        }

        // Auto-check update khi khởi động (background)
        _ = CheckUpdateOnStartupAsync();

        base.OnFrameworkInitializationCompleted();
    }

    private void ToggleIME()
    {
        if (_hook != null)
        {
            _hook.IsEnabled = !_hook.IsEnabled;
        }
    }

    private void SetEngine(string engineName)
    {
        if (_hook == null) return;

        _hook.Engine = engineName switch
        {
            "VNI" => new VniEngine(),
            _ => new TelexEngine()
        };

        UpdateTrayIcon();
    }

    private void ShowSettings()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = new MainWindow(_hook);
        }

        if (_settingsWindow.IsVisible)
        {
            _settingsWindow.Activate();
        }
        else
        {
            _settingsWindow.RefreshState();
            _settingsWindow.Show();
            _settingsWindow.Activate();
        }
    }

    private void ExitApp()
    {
        Console.WriteLine("[VietIME] Đang thoát...");

        // 1. Dừng hook trước
        try
        {
            _hook?.Dispose();
            _hook = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VietIME] Hook dispose error: {ex.Message}");
        }

        // 2. Đóng settings window nếu đang mở
        try
        {
            _settingsWindow?.Close();
            _settingsWindow = null;
        }
        catch { }

        // 3. Xóa tray icon
        try
        {
            var icons = TrayIcon.GetIcons(this);
            if (icons != null)
            {
                foreach (var icon in icons)
                {
                    icon.Dispose();
                }
            }
        }
        catch { }

        // 4. Shutdown Avalonia
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.TryShutdown(0);
        }

        // 5. Force exit — LSUIElement app không có main window nên TryShutdown
        //    có thể không kết thúc process. Dùng Environment.Exit làm safety net.
        Environment.Exit(0);
    }

    private void UpdateTrayIcon()
    {
        var icons = TrayIcon.GetIcons(this);
        if (icons != null && icons.Count > 0)
        {
            var enabled = _hook?.IsEnabled ?? false;
            var engineName = _hook?.Engine?.Name ?? "Telex";

            // Cập nhật tooltip
            icons[0].ToolTipText = enabled
                ? $"VietIME — Bật ({engineName})"
                : "VietIME — Tắt";

            // Swap icon: đỏ khi bật, xám khi tắt
            try
            {
                var iconPath = enabled
                    ? "avares://VietIME/Assets/tray-icon-on.png"
                    : "avares://VietIME/Assets/tray-icon-off.png";
                var uri = new Uri(iconPath);
                var assets = Avalonia.Platform.AssetLoader.Open(uri);
                icons[0].Icon = new WindowIcon(assets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VietIME] Icon update error: {ex.Message}");
            }
        }
    }

    private void Hook_EnabledChanged(object? sender, bool enabled)
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            UpdateTrayIcon();
        });
    }

    private void Hook_Error(object? sender, string error)
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Hiện error dialog
            if (_settingsWindow == null)
            {
                _settingsWindow = new MainWindow(_hook);
            }
            _settingsWindow.Show();
            _settingsWindow.ShowError(error);
        });
    }

    /// <summary>
    /// Kiểm tra update khi khởi động app.
    /// Nếu có bản mới → tự động mở Settings window để hiện thông báo.
    /// </summary>
    private async Task CheckUpdateOnStartupAsync()
    {
        try
        {
            // Đợi 3 giây sau khi app khởi động để không ảnh hưởng startup
            await Task.Delay(3000);

            var updateService = new UpdateService();
            var info = await updateService.CheckForUpdateAsync();

            if (info.HasUpdate)
            {
                Console.WriteLine($"[VietIME] Có phiên bản mới: v{info.LatestVersion}");

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Mở Settings window và hiện update banner
                    if (_settingsWindow == null)
                    {
                        _settingsWindow = new MainWindow(_hook);
                    }

                    if (!_settingsWindow.IsVisible)
                    {
                        _settingsWindow.RefreshState();
                        _settingsWindow.Show();
                    }
                    _settingsWindow.Activate();
                });
            }
            else
            {
                Console.WriteLine($"[VietIME] Đang dùng phiên bản mới nhất: v{UpdateService.AppVersion}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VietIME] Lỗi kiểm tra update: {ex.Message}");
        }
    }
}

/// <summary>
/// Simple ICommand implementation cho NativeMenu bindings
/// </summary>
internal class RelayCommand : ICommand
{
    private readonly Action _execute;
#pragma warning disable CS0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

    public RelayCommand(Action execute)
    {
        _execute = execute;
    }

    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
}
