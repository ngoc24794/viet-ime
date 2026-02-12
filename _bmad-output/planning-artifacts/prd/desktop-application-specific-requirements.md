# Desktop Application Specific Requirements

### Project-Type Overview
VietIME Rust is a native Windows desktop application (MVP) with cross-platform architecture designed for future macOS/Linux support. Built with Rust for performance, small binary size, and single-file deployment.

### Technical Architecture Considerations

**Modular Workspace Structure:**
```
vietime-rust/
├── core/           # Platform-agnostic Vietnamese input engine
│   ├── engines/      # Telex, VNI input processing
│   └── models/       # Vowel rules, tone placement logic
├── platforms/       # OS-specific implementations
│   ├── windows/      # Win32 API (SetWindowsHookEx)
│   ├── macos/        # CGEvent tap (future)
│   └── linux/        # evdev (future)
└── ui/             # Cross-platform UI (Slint)
```

**Platform Abstraction Layer:**
```rust
pub trait PlatformHook {
    fn install(&mut self, callback: InputCallback) -> Result<()>;
    fn uninstall(&mut self) -> Result<()>;
    fn send_text(&self, text: &str) -> Result<()>;
}
```
Each platform implements this trait for consistent behavior.

### Platform Support Requirements

**MVP - Windows:**
- Windows 10/11 (64-bit) only
- Single portable EXE, no installation required
- .NET runtime NOT required
- Win32 API: SetWindowsHookEx for keyboard interception

**Future - Cross-Platform:**
- macOS: CGEvent tap for keyboard interception
- Linux: evdev for device access, uinput for text output

### System Integration Requirements

**MVP Requirements:**
- **Auto-start**: Optional Registry Run key for Windows startup
- **System tray**: Always-visible tray icon with V indicator
- **Process priority**: Normal priority, no admin elevation required

**Post-MVP Features:**
- **Caps Lock LED**: Toggle LED indicator for VI/EN mode status
- **Toast notifications**: Silent update completion, error alerts

### Offline Capabilities

**Architecture:**
- Fully offline-capable - no internet connection required for core functionality
- Update check runs on app startup, gracefully fails if offline
- No telemetry, license checks, or online-only features

### Implementation Considerations

**Configuration Management:**
- TOML-based configuration file for cross-platform compatibility
- Settings stored in user AppData (Windows: `%APPDATA%\VietIME\`)
- Live reload on file change (Post-MVP)

**Update Strategy:**
- Silent background updates on app startup
- Binary replacement on next app launch
- Update endpoint: GitHub Releases API

---
