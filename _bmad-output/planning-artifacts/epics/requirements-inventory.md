# Requirements Inventory

### Functional Requirements

**Input Processing:**
- **FR1:** User can type Vietnamese characters using Telex input method
- **FR2:** System can process tone marks (s, f, r, x, j) and place correctly on vowels
- **FR3:** System can process special characters (w, [, ]) to create ơ, ư, đ
- **FR4:** System can handle backspace to undo previous keystrokes within current buffer

**Mode Management:**
- **FR5:** User can toggle between Vietnamese (VI) and English (EN) input modes
- **FR6:** User can identify current input mode via system tray icon indicator
- **FR7:** System can maintain input mode state across application switches

**Application Lifecycle:**
- **FR8:** User can launch application by double-clicking portable EXE without installation
- **FR9:** System can initialize and become ready for input within 300ms of launch
- **FR10:** User can terminate application via system tray context menu

**Text Output:**
- **FR11:** System can output processed Vietnamese text to active application cursor position
- **FR12:** System can send text via clipboard simulation (Ctrl+V) for terminal compatibility

**Configuration:**
- **FR13:** System can save user preferences (input mode, hotkey settings) to persistent storage
- **FR14:** System can load user preferences on application launch

**System Integration:**
- **FR15:** User can see application indicator in Windows system tray
- **FR16:** User can access context menu via right-click on system tray icon
- **FR17:** User can toggle input modes via keyboard shortcut (Ctrl+Shift)

**Error Handling:**
- **FR18:** System can recover from input errors via application restart
- **FR19:** User can report bugs via GitHub Issues workflow

### NonFunctional Requirements

**Performance:**
- **NFR-Performance-01:** System shall process keystrokes and output Vietnamese characters within 5ms of keypress (95th percentile < 5ms, 99th percentile < 10ms) - Tested with rapid typing (15+ chars/second)
- **NFR-Performance-02:** System shall achieve ready state within 300ms for 95% of launches across Windows 10/11 environments - Ready state = tray icon visible + keyboard hook installed
- **NFR-Performance-03:** System shall process consecutive keystrokes at 15+ characters/second with zero dropped keystrokes - Buffer timeout: 2 seconds of inactivity triggers reset
- **NFR-Performance-04:** System shall maintain CPU usage < 1% when idle and memory footprint < 15MB during normal operation - Idle state = no input processing for > 5 seconds

**Scalability:**
- **NFR-Scalability-01:** System shall support automatic updates for 5,000+ users without degraded performance - Update check throttled to once per 24 hours - GitHub Releases API used

### Additional Requirements

**From Architecture:**

**Modular Workspace Structure:**
- Cargo workspace with three main components: `core/` (platform-agnostic input engine), `platforms/` (OS-specific implementations), `ui/` (cross-platform UI)
- Core engine contains: `engines/` (Telex, VNI input processing), `models/` (Vowel rules, tone placement logic)

**Technology Stack:**
- Language: Rust for performance, memory safety, and small binary size
- Win32 API: `windows-rs` crate (official Microsoft Rust bindings)
- UI Framework: `slint` for cross-platform native performance
- Config Format: TOML for cross-platform compatibility
- Config Location: `%APPDATA%\VietIME\` on Windows

**Platform Abstraction:**
- `PlatformHook` trait for consistent behavior across platforms
- Trait methods: `install()`, `uninstall()`, `send_text()`, `platform_name()`
- Windows: `SetWindowsHookEx(WH_KEYBOARD_LL)` for hook, clipboard-based output for terminal compatibility
- Future platforms: macOS (CGEvent tap), Linux (evdev)

**Buffer Management:**
- Zero-allocation buffer with fixed capacity (MAX_BUFFER_SIZE = 20 chars)
- Buffer timeout: 2 seconds of inactivity triggers reset
- Array-based buffer: `[char; MAX_BUFFER_SIZE]` with length tracking

**Error Handling:**
- Domain-specific error types: `CoreError`, `PlatformError`
- Result<T, E> pattern with early return using `?` operator
- Context added via `.context()` for error chain

**Naming Conventions (Rust RFC 430):**
- Functions/Variables: `snake_case` (e.g., `process_key()`, `current_buffer`)
- Types/Structs: `PascalCase` (e.g., `VietnameseChar`, `InputEngine`)
- Constants: `SCREAMING_SNAKE_CASE` (e.g., `MAX_BUFFER_SIZE`, `BUFFER_TIMEOUT_MS`)
- Engine Classes: `<Method>Engine` (e.g., `TelexEngine`, `VniEngine`)
- Tone Marks: `Tone::<Name>` (e.g., `Tone::Acute`, `Tone::Grave`)
- Vowel Types: `Vowel::<Description>` (e.g., `Vowel::Basic`, `Vowel::WithDieresis`)

**Testing Strategy:**
- Unit tests per module (engines, models)
- Integration tests for cross-component (Engine + Hook)
- Performance tests for NFR compliance
- Edge case tests for user-reported bugs

**From UX Design:**

**System Tray Integration:**
- Primary touchpoint and status indicator
- Crystal-clear visual distinction between VI (#2563EB - Vietnamese Blue) and EN (#94A3B8 - Slate Gray) states
- Context menu on right-click with Toggle VI/EN, Settings, Exit options
- Instant visual feedback on mode switch

**Keyboard-First Interaction:**
- Mode switching via single keyboard shortcut (Ctrl+Shift) with instant feedback
- Design for power users who prefer keyboard over mouse
- No typing lag, no interruption to typing flow

**Terminal Compatibility:**
- Clipboard-based output must feel instant and natural
- First-time success in terminal is make-or-break moment
- Zero errors in full sentence with complex tones

**Zero-Friction Deployment:**
- Double-click EXE → start typing Vietnamese immediately
- No installation wizard required
- No configuration needed for first-time success
- Portable simplicity: share file, it works

**Silent Operation:**
- Application should feel invisible during normal typing
- Only visible when needed (mode indication, tray icon, notifications)
- When it works perfectly, users forget it's running

**Design System:**
- Primary Color: #2563EB (Vietnamese Blue)
- Secondary Color: #64748B (Slate Gray)
- VI Mode (Active): #2563EB
- EN Mode (Inactive): #94A3B8
- Typefaces: Segoe UI (Windows), San Francisco (macOS), Inter (Linux)
- Spacing Unit: 4px
- Minimum touch target: 44px for mouse interaction

**Component Implementation Phases:**
- Phase 1 (MVP): SystemTrayIcon, SettingsWindow, ContextMenu, NotificationOverlay (optional)
- Phase 2 (Post-MVP): Enhanced ModeIndicator, HotkeyInput, EncodingSelector
- Phase 3 (Future): MacroEditor, CustomHotkey, DarkMode

### FR Coverage Map

FR1: Epic 1 - Telex input method processing
FR2: Epic 1 - Tone marks (s,f,r,x,j) placement
FR3: Epic 1 - Special characters (w,[,]) for ơ,ư,đ
FR4: Epic 1 - Backspace handling in buffer
FR5: Epic 2 - Toggle between VI/EN modes
FR6: Epic 2 - Mode indicator in tray icon
FR7: Epic 2 - Maintain mode across app switches
FR8: Epic 3 - Launch via portable EXE
FR9: Epic 3 - Ready within 300ms
FR10: Epic 3 - Terminate via tray menu
FR11: Epic 1 - Output Vietnamese text to cursor
FR12: Epic 1 - Clipboard-based (Ctrl+V) terminal output
FR13: Epic 4 - Save preferences to storage
FR14: Epic 4 - Load preferences on launch
FR15: Epic 2 - Tray icon display
FR16: Epic 2 - Context menu on right-click
FR17: Epic 2 - Mode toggle via shortcut
FR18: Epic 4 - Recover from errors via restart
FR19: Epic 4 - Bug reporting via GitHub
