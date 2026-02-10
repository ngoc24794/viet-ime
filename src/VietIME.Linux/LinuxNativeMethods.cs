using System.Runtime.InteropServices;

namespace VietIME.Linux;

/// <summary>
/// P/Invoke declarations cho Linux evdev/uinput.
/// Tương đương NativeMethods.cs (Win) và MacNativeMethods.cs (Mac).
/// </summary>
public static class LinuxNativeMethods
{
    private const string LibC = "libc";

    // ═══════════════════════════════════════════
    // File operations
    // ═══════════════════════════════════════════

    [DllImport(LibC, SetLastError = true)]
    public static extern int open([MarshalAs(UnmanagedType.LPStr)] string pathname, int flags);

    [DllImport(LibC, SetLastError = true)]
    public static extern int close(int fd);

    [DllImport(LibC, SetLastError = true)]
    public static extern nint read(int fd, ref InputEvent buf, nint count);

    [DllImport(LibC, SetLastError = true)]
    public static extern nint write(int fd, ref InputEvent buf, nint count);

    [DllImport(LibC, SetLastError = true)]
    public static extern nint write(int fd, ref UinputSetup buf, nint count);

    [DllImport(LibC, SetLastError = true)]
    public static extern int ioctl(int fd, ulong request, int value);

    [DllImport(LibC, SetLastError = true)]
    public static extern int ioctl(int fd, ulong request, ref UinputSetup value);

    // ═══════════════════════════════════════════
    // Open flags
    // ═══════════════════════════════════════════

    public const int O_RDONLY = 0x0000;
    public const int O_WRONLY = 0x0001;
    public const int O_RDWR = 0x0002;
    public const int O_NONBLOCK = 0x0800;

    // ═══════════════════════════════════════════
    // evdev ioctl
    // ═══════════════════════════════════════════

    // EVIOCGRAB - grab/ungrab device
    public const ulong EVIOCGRAB = 0x40044590;

    // EVIOCGBIT(ev, len) = _IOC(_IOC_READ, 'E', 0x20 + ev, len)
    // For EV_KEY (0x01): EVIOCGBIT(1, KEY_MAX/8+1)
    // _IOC_READ = 2, 'E' = 0x45
    // _IOC(2, 0x45, 0x21, 96) on 64-bit
    public static ulong EVIOCGBIT(int ev, int len)
    {
        return (2UL << 30) | ((ulong)(byte)'E' << 8) | (ulong)(0x20 + ev) | ((ulong)len << 16);
    }

    // EVIOCGNAME(len) = _IOC(_IOC_READ, 'E', 0x06, len)
    public static ulong EVIOCGNAME(int len)
    {
        return (2UL << 30) | ((ulong)(byte)'E' << 8) | 0x06UL | ((ulong)len << 16);
    }

    [DllImport(LibC, SetLastError = true)]
    public static extern int ioctl(int fd, ulong request, byte[] value);

    // ═══════════════════════════════════════════
    // uinput ioctl
    // ═══════════════════════════════════════════

    // UI_DEV_SETUP = _IOW('U', 3, struct uinput_setup) 
    public const ulong UI_DEV_SETUP = 0x405C5503;
    // UI_DEV_CREATE
    public const ulong UI_DEV_CREATE = 0x5501;
    // UI_DEV_DESTROY
    public const ulong UI_DEV_DESTROY = 0x5502;
    // UI_SET_EVBIT
    public const ulong UI_SET_EVBIT = 0x40045564;
    // UI_SET_KEYBIT
    public const ulong UI_SET_KEYBIT = 0x40045565;

    // ═══════════════════════════════════════════
    // Event types (EV_*)
    // ═══════════════════════════════════════════

    public const ushort EV_SYN = 0x00;
    public const ushort EV_KEY = 0x01;
    public const ushort EV_REP = 0x14;

    // SYN codes
    public const ushort SYN_REPORT = 0x00;

    // Key states
    public const int KEY_STATE_RELEASE = 0;
    public const int KEY_STATE_PRESS = 1;
    public const int KEY_STATE_REPEAT = 2;

    // ═══════════════════════════════════════════
    // Key codes (KEY_*)
    // ═══════════════════════════════════════════

    public const ushort KEY_RESERVED = 0;
    public const ushort KEY_ESC = 1;
    public const ushort KEY_1 = 2;
    public const ushort KEY_2 = 3;
    public const ushort KEY_3 = 4;
    public const ushort KEY_4 = 5;
    public const ushort KEY_5 = 6;
    public const ushort KEY_6 = 7;
    public const ushort KEY_7 = 8;
    public const ushort KEY_8 = 9;
    public const ushort KEY_9 = 10;
    public const ushort KEY_0 = 11;
    public const ushort KEY_MINUS = 12;
    public const ushort KEY_EQUAL = 13;
    public const ushort KEY_BACKSPACE = 14;
    public const ushort KEY_TAB = 15;
    public const ushort KEY_Q = 16;
    public const ushort KEY_W = 17;
    public const ushort KEY_E = 18;
    public const ushort KEY_R = 19;
    public const ushort KEY_T = 20;
    public const ushort KEY_Y = 21;
    public const ushort KEY_U = 22;
    public const ushort KEY_I = 23;
    public const ushort KEY_O = 24;
    public const ushort KEY_P = 25;
    public const ushort KEY_LEFTBRACE = 26;
    public const ushort KEY_RIGHTBRACE = 27;
    public const ushort KEY_ENTER = 28;
    public const ushort KEY_LEFTCTRL = 29;
    public const ushort KEY_A = 30;
    public const ushort KEY_S = 31;
    public const ushort KEY_D = 32;
    public const ushort KEY_F = 33;
    public const ushort KEY_G = 34;
    public const ushort KEY_H = 35;
    public const ushort KEY_J = 36;
    public const ushort KEY_K = 37;
    public const ushort KEY_L = 38;
    public const ushort KEY_SEMICOLON = 39;
    public const ushort KEY_APOSTROPHE = 40;
    public const ushort KEY_GRAVE = 41;
    public const ushort KEY_LEFTSHIFT = 42;
    public const ushort KEY_BACKSLASH = 43;
    public const ushort KEY_Z = 44;
    public const ushort KEY_X = 45;
    public const ushort KEY_C = 46;
    public const ushort KEY_V = 47;
    public const ushort KEY_B = 48;
    public const ushort KEY_N = 49;
    public const ushort KEY_M = 50;
    public const ushort KEY_COMMA = 51;
    public const ushort KEY_DOT = 52;
    public const ushort KEY_SLASH = 53;
    public const ushort KEY_RIGHTSHIFT = 54;
    public const ushort KEY_LEFTALT = 56;
    public const ushort KEY_SPACE = 57;
    public const ushort KEY_CAPSLOCK = 58;
    public const ushort KEY_RIGHTCTRL = 97;
    public const ushort KEY_RIGHTALT = 100;
    public const ushort KEY_HOME = 102;
    public const ushort KEY_UP = 103;
    public const ushort KEY_PAGEUP = 104;
    public const ushort KEY_LEFT = 105;
    public const ushort KEY_RIGHT = 106;
    public const ushort KEY_END = 107;
    public const ushort KEY_DOWN = 108;
    public const ushort KEY_PAGEDOWN = 109;
    public const ushort KEY_DELETE = 111;
    public const ushort KEY_MAX = 767;

    // ═══════════════════════════════════════════
    // Structs
    // ═══════════════════════════════════════════

    /// <summary>
    /// struct input_event (24 bytes on 64-bit)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct InputEvent
    {
        public long TimeSec;   // time.tv_sec
        public long TimeUsec;  // time.tv_usec
        public ushort Type;
        public ushort Code;
        public int Value;
    }

    /// <summary>
    /// struct input_id
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct InputId
    {
        public ushort BusType;
        public ushort Vendor;
        public ushort Product;
        public ushort Version;
    }

    /// <summary>
    /// struct uinput_setup
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct UinputSetup
    {
        public InputId Id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string Name;
        public uint FfEffectsMax;
    }

    // ═══════════════════════════════════════════
    // Key code → char mapping (US QWERTY layout)
    // ═══════════════════════════════════════════

    private static readonly Dictionary<ushort, (char normal, char shifted)> KeyCharMap = new()
    {
        [KEY_A] = ('a', 'A'), [KEY_B] = ('b', 'B'), [KEY_C] = ('c', 'C'),
        [KEY_D] = ('d', 'D'), [KEY_E] = ('e', 'E'), [KEY_F] = ('f', 'F'),
        [KEY_G] = ('g', 'G'), [KEY_H] = ('h', 'H'), [KEY_I] = ('i', 'I'),
        [KEY_J] = ('j', 'J'), [KEY_K] = ('k', 'K'), [KEY_L] = ('l', 'L'),
        [KEY_M] = ('m', 'M'), [KEY_N] = ('n', 'N'), [KEY_O] = ('o', 'O'),
        [KEY_P] = ('p', 'P'), [KEY_Q] = ('q', 'Q'), [KEY_R] = ('r', 'R'),
        [KEY_S] = ('s', 'S'), [KEY_T] = ('t', 'T'), [KEY_U] = ('u', 'U'),
        [KEY_V] = ('v', 'V'), [KEY_W] = ('w', 'W'), [KEY_X] = ('x', 'X'),
        [KEY_Y] = ('y', 'Y'), [KEY_Z] = ('z', 'Z'),
        [KEY_1] = ('1', '!'), [KEY_2] = ('2', '@'), [KEY_3] = ('3', '#'),
        [KEY_4] = ('4', '$'), [KEY_5] = ('5', '%'), [KEY_6] = ('6', '^'),
        [KEY_7] = ('7', '&'), [KEY_8] = ('8', '*'), [KEY_9] = ('9', '('),
        [KEY_0] = ('0', ')'), [KEY_MINUS] = ('-', '_'), [KEY_EQUAL] = ('=', '+'),
        [KEY_LEFTBRACE] = ('[', '{'), [KEY_RIGHTBRACE] = (']', '}'),
        [KEY_SEMICOLON] = (';', ':'), [KEY_APOSTROPHE] = ('\'', '"'),
        [KEY_GRAVE] = ('`', '~'), [KEY_BACKSLASH] = ('\\', '|'),
        [KEY_COMMA] = (',', '<'), [KEY_DOT] = ('.', '>'), [KEY_SLASH] = ('/', '?'),
        [KEY_SPACE] = (' ', ' '),
    };

    /// <summary>
    /// Chuyển key code sang ký tự (US QWERTY).
    /// Tương đương NativeMethods.VirtualKeyToChar() trên Windows.
    /// </summary>
    public static char? KeyCodeToChar(ushort keyCode, bool isShift)
    {
        if (KeyCharMap.TryGetValue(keyCode, out var pair))
            return isShift ? pair.shifted : pair.normal;
        return null;
    }

    /// <summary>
    /// Chuyển ký tự thành key code + shift state.
    /// Dùng khi cần gửi ký tự qua uinput (cho output text).
    /// </summary>
    public static (ushort keyCode, bool needShift)? CharToKeyCode(char c)
    {
        foreach (var (keyCode, (normal, shifted)) in KeyCharMap)
        {
            if (normal == c) return (keyCode, false);
            if (shifted == c) return (keyCode, true);
        }
        return null;
    }
}
