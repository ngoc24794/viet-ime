# Project Context Analysis

### Requirements Overview

**Functional Requirements:**

VietIME Rust requires 19 functional requirements across 7 core domains:

1. **Input Processing (FR1-FR4):**
   - Telex input method with tone mark processing (s, f, r, x, j)
   - Special character handling (w, [, ]) for ơ, ư, đ
   - Backspace undo within current buffer
   - VNI input method support

2. **Mode Management (FR5-FR7):**
   - Toggle between Vietnamese (VI) and English (EN) modes
   - Visual indicator via system tray icon
   - State persistence across application switches

3. **Application Lifecycle (FR8-FR10):**
   - Portable EXE launch without installation
   - Initialization within 300ms
   - Graceful termination via system tray

4. **Text Output (FR11-FR12):**
   - Output processed Vietnamese text to active cursor position
   - Clipboard simulation (Ctrl+V) for terminal compatibility

5. **Configuration (FR13-FR14):**
   - Persistent storage for user preferences
   - Load preferences on launch

6. **System Integration (FR15-FR17):**
   - Windows system tray integration
   - Context menu access
   - Keyboard shortcut toggle (Ctrl+Shift)

7. **Error Handling (FR18-FR19):**
   - Recovery from input errors via restart
   - Bug reporting workflow

**Non-Functional Requirements:**

Performance NFRs that will drive architectural decisions:

- **Input Latency:** < 5ms (95th percentile), < 10ms (99th percentile) for keystroke processing
- **Startup Time:** < 300ms to ready state (tray icon visible + hook installed)
- **Throughput:** 15+ characters/second continuous typing with zero dropped keystrokes
- **Resource Usage:** CPU < 1% idle, memory < 15MB
- **Binary Size:** Single portable EXE < 5MB
- **Buffer Timeout:** 2 seconds of inactivity triggers reset

**Scale & Complexity:**

- Primary domain: **Native Desktop Application** (Windows MVP, cross-platform vision)
- Complexity level: **Medium**
- Estimated architectural components: ~8-10 core modules

### Technical Constraints & Dependencies

**From Desktop Application Requirements:**

**MVP - Windows Only:**
- Windows 10/11 (64-bit) only
- Single portable EXE, no installation required
- No .NET runtime dependency
- Win32 API via `windows-rs` crate for `SetWindowsHookEx`

**Rust-Specific Architecture:**
- **Modular Workspace Structure:**
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

- **Platform Abstraction Layer:**
  ```rust
  pub trait PlatformHook {
      fn install(&mut self, callback: InputCallback) -> Result<()>;
      fn uninstall(&mut self) -> Result<()>;
      fn send_text(&self, text: &str) -> Result<()>;
  }
  ```

- **UI Framework:** `slint` for cross-platform tray application

**Configuration & Deployment:**
- TOML-based configuration for cross-platform compatibility
- Settings stored in user AppData (Windows: `%APPDATA%\VietIME\`)
- Silent updates via GitHub Releases API

### Cross-Cutting Concerns Identified

**1. Input Buffer Management:**
- State persistence across keystrokes
- 2-second timeout for buffer reset
- Backspace undo logic within buffer
- Context switch handling (app switch → buffer reset)

**2. Platform Abstraction:**
- Keyboard hook interface varies by OS (Win32, CGEvent, evdev)
- Text output method varies by OS (SendInput, clipboard, uinput)
- Consistent behavior across platforms via `PlatformHook` trait

**3. Performance Critical Path:**
- Keypress → Hook → Process → Output must complete in < 5ms
- No allocation in hot path (buffer management)
- Efficient Unicode character conversion

**4. Configuration Management:**
- TOML parsing and serialization
- Live reload on file change (post-MVP)
- Default preferences for first-time users

**5. Error Recovery:**
- Graceful degradation on hook failure
- Restart capability from error state
- Bug reporting workflow for edge cases

---
