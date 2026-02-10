using Avalonia;
using VietIME.Core.Engines;
using VietIME.Linux;

namespace VietIME.Linux.App;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--test-engine")
        {
            RunEngineTest();
            return;
        }

        if (args.Length > 0 && args[0] == "--test-hook")
        {
            RunHookTest();
            return;
        }

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

    /// <summary>
    /// Test TelexEngine trực tiếp trong console — không cần evdev.
    /// Gõ từng ký tự và xem engine xử lý thế nào.
    /// </summary>
    static void RunEngineTest()
    {
        Console.WriteLine("=== VietIME Engine Test Mode ===");
        Console.WriteLine("Gõ từng ký tự để test. Nhấn Enter để reset, Ctrl+C để thoát.");
        Console.WriteLine("Ví dụ: gõ 'V', 'i', 'e', 'e', 't', 'j' → Việt");
        Console.WriteLine();

        var engine = new TelexEngine();
        Console.Write("> ");

        while (true)
        {
            var keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                engine.Reset();
                Console.WriteLine();
                Console.WriteLine("--- Reset ---");
                Console.Write("> ");
                continue;
            }

            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                engine.ProcessBackspace();
                Console.Write("\b \b");
                Console.WriteLine($"  [Buffer: '{engine.GetBuffer()}']");
                Console.Write("> ");
                continue;
            }

            char ch = keyInfo.KeyChar;
            if (ch == 0) continue;

            bool isShift = (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0;
            var result = engine.ProcessKey(ch, isShift);

            if (result.Handled && result.OutputText != null)
            {
                Console.Write($"  [Handled] BS={result.BackspaceCount}, Output='{result.OutputText}', Buffer='{engine.GetBuffer()}'");
                Console.WriteLine();
                Console.Write("> ");
            }
            else
            {
                Console.Write($"{ch}");
                Console.Write($"  [Pass] Buffer='{engine.GetBuffer()}'");
                Console.WriteLine();
                Console.Write("> ");
            }
        }
    }

    /// <summary>
    /// Test evdev hook — tạo hook và log phím.
    /// Dùng để debug quyền /dev/input.
    /// </summary>
    static void RunHookTest()
    {
        Console.WriteLine("=== VietIME Hook Test Mode (Linux) ===");
        Console.WriteLine("Test evdev + uinput + TelexEngine. Ctrl+C để thoát.");
        Console.WriteLine();

        // Kiểm tra keyboard device
        var device = LinuxKeyboardHook.FindKeyboardDevice();
        Console.WriteLine($"Keyboard device: {device ?? "KHÔNG TÌM THẤY"}");
        if (device == null)
        {
            Console.WriteLine("Cần quyền: sudo usermod -aG input $USER && logout");
            return;
        }
        Console.WriteLine();

        using var hook = new LinuxKeyboardHook();
        hook.Engine = new TelexEngine();
        hook.DebugLog += msg => Console.WriteLine($"[Hook] {msg}");
        hook.EnabledChanged += (_, enabled) => Console.WriteLine($"[State] Enabled={enabled}");
        hook.Error += (_, err) => Console.WriteLine($"[Error] {err}");

        hook.Install();
        Console.WriteLine("Hook installed. Gõ phím để test...");
        Console.WriteLine("(Log sẽ hiển thị mỗi phím và kết quả engine)");

        // Giữ app chạy
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        try { Task.Delay(-1, cts.Token).Wait(); }
        catch (AggregateException) { }

        hook.Uninstall();
        Console.WriteLine("\nHook uninstalled. Bye!");
    }
}
