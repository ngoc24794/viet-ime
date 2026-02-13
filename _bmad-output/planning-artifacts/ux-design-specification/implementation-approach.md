# Implementation Approach

**Phase 1 - Core Components** (MVP Release)

| Component           | Implementation Notes                     |
|---------------------|----------------------------------------|
| SystemTrayIcon    | Slint native - Windows-only MVP            |
| SettingsWindow     | 3-tab vertical layout                    |
| ContextMenu       | Right-click context menu                   |
| NotificationOverlay| Optional popup notifications (user pref) |
| Button          | Standard primary (Save), secondary (Cancel) |

**Phase 2 - Supporting Components** (Post-MVP)

| Component        | Implementation Notes                     |
|---------------------|----------------------------------------|
| ModeIndicator   | Enhanced tray icon with VI/EN state    |
| HotkeyInput    | Text input with validation           |
| EncodingSelector| Dropdown with UTF-8 default (TOML) |

**Phase 3 - Enhancement Components** (Future)

| Component      | Implementation Notes                     |
|---------------------|----------------------------------------|
| MacroEditor    | Advanced feature, power users             |
| CustomHotkey   | Per-action hotkey configuration          |

### Component Implementation Strategy

**Phase 1 - Core Components** (MVP Release)

| Purpose | Provide essential Vietnamese typing functionality |

| Component      | Design Priority | Platform Notes                |
|---------------------|-----------|----------|---------------|----------|
| Input Engine    | CRITICAL - Core Telex/VNI engines  |
| KeyboardHook  | CRITICAL - Low-level Win32 API (must prevent GC) |
| Clipboard     | CRITICAL - Terminal compatibility via Ctrl+V  |
| SettingsWindow | HIGH     - Primary UI, must meet accessibility |
| SystemTrayIcon  | CRITICAL - Only visible UI element in system tray |

**Phase 2 - Supporting Components** (Post-MVP)

| Component      | Purpose                                      | Design Priority | Platform Notes        |
|---------------------|-----------|----------|---------------|----------|---------------|---------------|
| ContextMenu    | MEDIUM    - Provide quick access to common actions  |
| Toast        | LOW      - Brief notifications for major events    |

**Phase 3 - Enhancement Components** (Future)

| Component      | Purpose                                      | Design Priority | Platform Notes        |
|---------------------|-----------|----------|---------------|----------|---------------|---------------|
| DarkMode     | ENHANCEMENT - Dark theme for reduced eye strain   | LOW          | User preference option     |

**Implementation Strategy:**
- **Foundation:** Slint native components (minimal desktop-first)
- **Customization:** Progressive enhancement where needed
- **Accessibility:** WCAG AA compliance maintained
- **Performance:** <5ms latency preserved (critical NFR)