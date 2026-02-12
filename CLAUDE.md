# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

VietIME - Cross-platform Vietnamese input method editor (IME) supporting Windows (WPF), macOS, and Linux (Avalonia). Replaces Unikey with system-wide input support for terminals, IDEs, and all applications.

**Target Framework**: .NET 8.0

## Build & Run Commands

```powershell
# Build entire solution
dotnet build

# Run Windows version (WPF)
dotnet run --project src/VietIME.App

# Run macOS version (Avalonia)
dotnet run --project src/VietIME.Mac.App

# Run Linux version (Avalonia)
dotnet run --project src/VietIME.Linux.App

# Build portable self-contained single-file EXE (Windows)
dotnet publish src/VietIME.App -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -o ./publish

# Test keyboard hook manually
dotnet run TestHook.cs
```

## Architecture

```
src/
├── VietIME.Core/         # Platform-agnostic Vietnamese input engine
│   ├── Engines/          # IInputEngine implementations (Telex, VNI)
│   ├── Models/           # VietnameseChar, Unicode mappings
│   └── Services/         # UpdateService
│
├── VietIME.Hook/         # Windows keyboard hook layer (Win32 API)
│   ├── KeyboardHook.cs   # Low-level keyboard hook via SetWindowsHookEx
│   └── NativeMethods.cs  # P/Invoke declarations (SendInput, VirtualKeyToChar)
│
├── VietIME.App/          # Windows WPF Application
│   ├── App.xaml.cs       # Entry point, system tray, engine management
│   └── MainWindow.xaml  # Settings UI
│
├── VietIME.Mac/          # macOS platform code (Avalonia)
├── VietIME.Mac.App/      # macOS Avalonia Application
├── VietIME.Linux/        # Linux platform code (Avalonia, evdev)
└── VietIME.Linux.App/    # Linux Avalonia Application
```

### Key Processing Flow

1. **Keyboard Hook**: `KeyboardHook.HookCallback()` intercepts all keystrokes via `SetWindowsHookEx(WH_KEYBOARD_LL)`
2. **Key Translation**: Virtual key → char via `NativeMethods.VirtualKeyToChar()`
3. **Engine Processing**: Call `IInputEngine.ProcessKey()` → returns `ProcessKeyResult`
4. **Text Output**: If `Handled=true`: send backspaces + output text via Clipboard (Ctrl+V) or SendInput

### Input Engine Interface

All input engines implement `IInputEngine`:
- `Name`: Engine identifier
- `ProcessKey(char, bool)`: Returns `ProcessKeyResult` with `Handled`, `BackspaceCount`, `OutputText`, `CurrentBuffer`
- `Reset()`: Clears buffer on context switch
- `ProcessBackspace()`: Handles backspace in buffer
- `GetBuffer()`: Returns current buffer state

### Text Output Methods

- **Clipboard method** (default): Backspaces + SetClipboard + Ctrl+V. Works with Terminals/PowerShell
- **SendInput method**: Uses `KEYEVENTF_UNICODE`. Faster but terminal-incompatible

## Adding a New Input Engine

1. Create class in `VietIME.Core/Engines/` implementing `IInputEngine`:

```csharp
public class MyEngine : IInputEngine
{
    public string Name => "MyEngine";

    public ProcessKeyResult ProcessKey(char key, bool isShiftPressed)
    {
        // Return: Handled (true to block original), BackspaceCount, OutputText
    }

    public void Reset() { /* Reset buffer on app switch */ }
    public bool ProcessBackspace() { /* Handle backspace */ }
    public string GetBuffer() { /* Return current buffer */ }
}
```

2. Register in `App.xaml.cs` `SetEngine()` method

## Tone Mark Rules

Tone mark placement logic in `TelexEngine.FindVowelPositionForTone()`:
- Vowels with accent/circumflex (ê, ô, ơ, â, ă, ư) have priority
- Special pattern: `ươ` → mark goes on `ơ` when followed by consonant
- `oa`, `oe`, `oă` → mark goes on second vowel
- Default: first vowel in consecutive vowel group

## Technical Notes

- Delegate `LowLevelKeyboardProc` must be kept as a field to prevent GC collection
- Extra info marker `0x56494D45` ("VIME") identifies self-generated input
- Buffer auto-resets after 2 seconds of inactivity
- Toggle hotkey: `Ctrl + Shift` (Windows), `⌘ + ~` (macOS), `Ctrl + ~` (Linux)
- Windows SmartScreen may warn due to missing code signature

## Platform-Specific Notes

- **Windows**: Uses `WH_KEYBOARD_LL` hook, requires reference preservation
- **macOS**: Uses Avalonia UI framework
- **Linux**: Uses evdev for input handling
