# Epic 2: Mode Switching & Visual Feedback

Users can toggle between VI/EN modes and CLEARLY SEE current mode status - no more uncertainty about which mode is active

**FRs covered:** FR5, FR6, FR7, FR15, FR16, FR17

### Story 2.1: System Tray Icon with Mode Indicator

As a Vietnamese typist,
I want to see the current input mode in the system tray,
So that I can instantly know whether I'm in VI or EN mode without guessing.

**Acceptance Criteria:**
**Given** the application is launched
**When** the application is ready
**Then** a tray icon appears in the Windows system tray
**And** the tray icon is visible within 300ms of launch (NFR-Performance-02)
**And** the icon displays the current input mode

**Given** Vietnamese (VI) mode is active
**When** I look at the system tray
**Then** the tray icon displays #2563EB (Vietnamese Blue)
**And** the visual indicator clearly shows "VI" state

**Given** English (EN) mode is active
**When** I look at the system tray
**Then** the tray icon displays #94A3B8 (Slate Gray)
**And** the visual indicator clearly shows "EN" state

**Given** I am switching between applications
**When** the active application changes
**Then** the tray icon remains visible and shows current mode
**And** mode is maintained across application switches (FR7)

---

### Story 2.2: Mode Toggle via Keyboard Shortcut

As a Vietnamese typist,
I want to toggle between VI and EN modes using a keyboard shortcut (Ctrl+Shift),
So that I can switch input modes without leaving the keyboard or interrupting my typing flow.

**Acceptance Criteria:**
**Given** the application is running and VI mode is active
**When** I press Ctrl+Shift
**Then** the mode switches to EN
**And** the tray icon changes to #94A3B8 (Slate Gray)
**And** visual feedback is instant

**Given** the application is running and EN mode is active
**When** I press Ctrl+Shift
**Then** the mode switches to VI
**And** the tray icon changes to #2563EB (Vietnamese Blue)
**And** visual feedback is instant

**Given** I am in the middle of typing text
**When** I press Ctrl+Shift to toggle mode
**Then** the mode change happens immediately
**And** no typing lag occurs
**And** my typing flow is not interrupted

**Given** the Win32 keyboard hook is installed
**When** the Ctrl+Shift combination is detected
**Then** the toggle is processed by the PlatformHook callback
**And** the hook prevents the keystroke from being passed to other applications

---

### Story 2.3: Context Menu for Quick Access

As a Vietnamese typist,
I want to access common actions via right-click on the tray icon,
So that I can quickly toggle modes, access settings, or exit the application.

**Acceptance Criteria:**
**Given** the application is running
**When** I right-click on the system tray icon
**Then** a context menu appears with the following options:
**And** "Toggle VI/EN" option is available
**And** "Settings" option is available
**And** "Exit" option is available

**Given** the context menu is displayed
**When** I click "Toggle VI/EN"
**Then** the input mode switches between VI and EN
**And** the tray icon updates to reflect the new mode

**Given** the context menu is displayed
**When** I click "Settings"
**Then** the Settings window opens
**And** I can configure preferences

**Given** the context menu is displayed
**When** I click "Exit"
**Then** the application terminates gracefully
**And** the keyboard hook is uninstalled
**And** the tray icon is removed

---

### Story 2.4: Mode State Persistence

As a Vietnamese typist,
I want the input mode to be maintained when I switch between applications,
So that I don't have to repeatedly re-select my preferred input mode.

**Acceptance Criteria:**
**Given** Vietnamese (VI) mode is active in Application A
**When** I switch to Application B
**Then** VI mode remains active
**And** the tray icon continues to show VI mode (#2563EB)

**Given** English (EN) mode is active in Application A
**When** I switch to Application B
**Then** EN mode remains active
**And** the tray icon continues to show EN mode (#94A3B8)

**Given** I have been using VI mode in my terminal
**When** I alt-tab to my web browser
**Then** VI mode is still active
**And** I can continue typing Vietnamese without mode change

**Given** the application loses and regains focus
**When** focus events are detected
**Then** the current mode state is preserved
**And** no mode reset occurs

---
