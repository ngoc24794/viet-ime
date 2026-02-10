using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using VietIME.Core.Engines;
using VietIME.Linux;

namespace VietIME.Linux.App;

public partial class App : Application
{
    private LinuxKeyboardHook? _hook;
    private MainWindow? _settingsWindow;

    // Commands cho NativeMenu bindings
    public ICommand ToggleCommand { get; }
    public ICommand SetTelexCommand { get; }
    public ICommand SetVniCommand { get; }
    public ICommand ShowSettingsCommand { get; }
    public ICommand ExitCommand { get; }

    public LinuxKeyboardHook? Hook => _hook;

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
            // Không hiện MainWindow khi khởi động — chỉ chạy trong tray
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        // Khởi tạo keyboard hook
        Console.WriteLine("[VietIME] Khởi tạo LinuxKeyboardHook...");
        _hook = new LinuxKeyboardHook();
        _hook.Engine = new TelexEngine();
        _hook.EnabledChanged += Hook_EnabledChanged;
        _hook.Error += Hook_Error;
        _hook.DebugLog += msg => Console.WriteLine($"[VietIME] {msg}");
        _hook.Install();
        Console.WriteLine("[VietIME] App đã khởi động. Tray icon trên system tray.");

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

        // 5. Force exit
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
