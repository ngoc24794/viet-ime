using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using VietIME.Core.Engines;
using VietIME.Hook;

namespace VietIME.App;

public partial class App : System.Windows.Application
{
    private KeyboardHook? _hook;
    private System.Windows.Forms.NotifyIcon? _trayIcon;
    private MainWindow? _settingsWindow;
    private DispatcherTimer? _layoutCheckTimer;

    public bool NotificationsEnabled { get; set; } = false;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        _hook = new KeyboardHook();
        _hook.Engine = new TelexEngine();
        _hook.EnabledChanged += Hook_EnabledChanged;
        _hook.Error += Hook_Error;
        _hook.Install();

        CreateTrayIcon();

        // Timer kiem tra keyboard layout moi 1 giay
        // Neu Unikey bat tieng Viet (layout 0x042A) -> tu dong tat VietIME
        _layoutCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _layoutCheckTimer.Tick += CheckKeyboardLayout;
        _layoutCheckTimer.Start();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        _layoutCheckTimer?.Stop();
        _hook?.Dispose();
        _trayIcon?.Dispose();
    }

    /// <summary>
    /// Kiem tra keyboard layout hien tai.
    /// Neu la Vietnamese (0x042A) thi Unikey dang bat -> tu dong tat VietIME de tranh xung dot.
    /// </summary>
    private void CheckKeyboardLayout(object? sender, EventArgs e)
    {
        if (_hook == null) return;

        try
        {
            var hWnd = NativeMethods.GetForegroundWindow();
            var threadId = NativeMethods.GetWindowThreadProcessId(hWnd, out _);
            var layout = NativeMethods.GetKeyboardLayout(threadId);

            // Layout ID: low word = language ID
            int langId = (int)layout & 0xFFFF;
            bool isVietnameseLayout = langId == 0x042A;

            if (isVietnameseLayout && _hook.IsEnabled)
            {
                // Unikey dang bat tieng Viet -> tat VietIME
                _hook.IsEnabled = false;
            }
        }
        catch
        {
            // Ignore errors
        }
    }

    private void CreateTrayIcon()
    {
        _trayIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = CreateIcon(),
            Text = "VietIME — Bật (Telex)",
            Visible = true
        };

        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.BackColor = Color.FromArgb(49, 50, 68);
        contextMenu.ForeColor = Color.FromArgb(205, 214, 244);
        contextMenu.Renderer = new DarkMenuRenderer();

        // Header: tên app + tác giả
        var headerItem = new System.Windows.Forms.ToolStripMenuItem("VietIME - © Đỗ Nam 2026");
        headerItem.Enabled = false;
        headerItem.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
        contextMenu.Items.Add(headerItem);

        // Header: mô tả
        var subtitleItem = new System.Windows.Forms.ToolStripMenuItem("Bộ gõ Tiếng Việt");
        subtitleItem.Enabled = false;
        subtitleItem.Font = new Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Italic);
        contextMenu.Items.Add(subtitleItem);

        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        var toggleItem = new System.Windows.Forms.ToolStripMenuItem("Bật/tắt VietIME");
        toggleItem.Click += (s, e) => ToggleIME();
        contextMenu.Items.Add(toggleItem);

        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        var telexItem = new System.Windows.Forms.ToolStripMenuItem("Telex");
        telexItem.Click += (s, e) => SetEngine("Telex");
        contextMenu.Items.Add(telexItem);

        var vniItem = new System.Windows.Forms.ToolStripMenuItem("VNI");
        vniItem.Click += (s, e) => SetEngine("VNI");
        contextMenu.Items.Add(vniItem);

        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        var settingsItem = new System.Windows.Forms.ToolStripMenuItem("Cài đặt...");
        settingsItem.Click += (s, e) => ShowSettings();
        contextMenu.Items.Add(settingsItem);

        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        var exitItem = new System.Windows.Forms.ToolStripMenuItem("Thoát");
        exitItem.Click += (s, e) => Shutdown();
        contextMenu.Items.Add(exitItem);

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += (s, e) => ToggleIME();
    }

    private Icon CreateIcon()
    {
        int size = 32;
        var bitmap = new Bitmap(size, size);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            float padding = size * 0.03f;
            float rectSize = size - padding * 2;
            float cornerRadius = size * 0.20f;

            // Dark rounded square background
            using var bgPath = CreateRoundedRectPath(padding, padding, rectSize, rectSize, cornerRadius);
            using var bgBrush = new SolidBrush(Color.FromArgb(20, 20, 28));
            g.FillPath(bgBrush, bgPath);

            // Subtle border
            using var borderPen = new Pen(Color.FromArgb(35, 255, 255, 255), 1f);
            g.DrawPath(borderPen, bgPath);

            // "V" - red when enabled, gray when disabled
            bool enabled = _hook?.IsEnabled ?? true;
            var vColor = enabled
                ? Color.FromArgb(215, 40, 40)
                : Color.FromArgb(100, 100, 110);

            float fontSize = size * 0.55f;
            using var font = new Font("Segoe UI", fontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
            var textSize = g.MeasureString("V", font);
            float x = (size - textSize.Width) / 2 + size * 0.015f;
            float y = (size - textSize.Height) / 2;

            // Subtle glow
            if (enabled)
            {
                var glowColor = Color.FromArgb(40, 215, 40, 40);
                using var glowBrush = new SolidBrush(glowColor);
                for (float offset = 1.2f; offset >= 0.5f; offset -= 0.35f)
                {
                    g.DrawString("V", font, glowBrush, x - offset, y);
                    g.DrawString("V", font, glowBrush, x + offset, y);
                    g.DrawString("V", font, glowBrush, x, y - offset);
                    g.DrawString("V", font, glowBrush, x, y + offset);
                }
            }

            using var textBrush = new SolidBrush(vColor);
            g.DrawString("V", font, textBrush, x, y);
        }

        return Icon.FromHandle(bitmap.GetHicon());
    }

    private static GraphicsPath CreateRoundedRectPath(float x, float y, float width, float height, float radius)
    {
        var path = new GraphicsPath();
        float d = radius * 2;
        path.AddArc(x, y, d, d, 180, 90);
        path.AddArc(x + width - d, y, d, d, 270, 90);
        path.AddArc(x + width - d, y + height - d, d, d, 0, 90);
        path.AddArc(x, y + height - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void UpdateTrayIcon()
    {
        if (_trayIcon == null) return;

        _trayIcon.Icon = CreateIcon();

        var enabled = _hook?.IsEnabled ?? false;
        var engineName = _hook?.Engine?.Name ?? "Telex";

        _trayIcon.Text = enabled
            ? $"VietIME — Bật ({engineName})"
            : "VietIME — Tắt";
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
        ShowBalloonTipIfEnabled("VietIME", $"Đã chuyển sang {engineName}");
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

    private void ShowBalloonTipIfEnabled(string title, string text)
    {
        if (!NotificationsEnabled) return;
        _trayIcon?.ShowBalloonTip(2000, title, text, System.Windows.Forms.ToolTipIcon.Info);
    }

    private void Hook_EnabledChanged(object? sender, bool enabled)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateTrayIcon();
            ShowBalloonTipIfEnabled("VietIME", enabled ? "Đã bật" : "Đã tắt");
        });
    }

    private void Hook_Error(object? sender, string error)
    {
        Dispatcher.Invoke(() =>
        {
            System.Windows.MessageBox.Show(error, "VietIME — Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }
}

internal class DarkMenuRenderer : System.Windows.Forms.ToolStripProfessionalRenderer
{
    public DarkMenuRenderer() : base(new DarkMenuColorTable()) { }

    protected override void OnRenderItemText(System.Windows.Forms.ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = Color.FromArgb(205, 214, 244);
        base.OnRenderItemText(e);
    }
}

internal class DarkMenuColorTable : ProfessionalColorTable
{
    public override Color MenuItemSelected => Color.FromArgb(69, 71, 90);
    public override Color MenuItemBorder => Color.FromArgb(69, 71, 90);
    public override Color MenuBorder => Color.FromArgb(69, 71, 90);
    public override Color MenuItemSelectedGradientBegin => Color.FromArgb(69, 71, 90);
    public override Color MenuItemSelectedGradientEnd => Color.FromArgb(69, 71, 90);
    public override Color MenuItemPressedGradientBegin => Color.FromArgb(88, 91, 112);
    public override Color MenuItemPressedGradientEnd => Color.FromArgb(88, 91, 112);
    public override Color MenuStripGradientBegin => Color.FromArgb(49, 50, 68);
    public override Color MenuStripGradientEnd => Color.FromArgb(49, 50, 68);
    public override Color ToolStripDropDownBackground => Color.FromArgb(49, 50, 68);
    public override Color ImageMarginGradientBegin => Color.FromArgb(49, 50, 68);
    public override Color ImageMarginGradientEnd => Color.FromArgb(49, 50, 68);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(49, 50, 68);
    public override Color SeparatorDark => Color.FromArgb(69, 71, 90);
    public override Color SeparatorLight => Color.FromArgb(69, 71, 90);
}
