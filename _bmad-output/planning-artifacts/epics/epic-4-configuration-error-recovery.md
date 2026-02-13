# Epic 4: Configuration & Error Recovery

User settings are persisted and the application recovers gracefully from errors - no loss of configuration data

**FRs covered:** FR13, FR14, FR18, FR19

### Story 4.1: TOML Configuration Storage

As a Vietnamese user,
I want my preferences to be saved automatically,
So that my settings are remembered when I restart the application.

**Acceptance Criteria:**
**Given** I have changed any settings (input mode, hotkey, etc.)
**When** the application closes or settings are applied
**Then** preferences are saved to config.toml in %APPDATA%\VietIME\
**And** the TOML format is used for cross-platform compatibility
**And** the directory is created if it doesn't exist

**Given** the configuration file is being saved
**When** write operations occur
**Then** the file is written atomically to prevent corruption
**And** a backup of the previous config is maintained
**And** write errors are logged and reported to the user

**Given** I have configured my preferred hotkey (Ctrl+Shift)
**When** the configuration is saved
**Then** the hotkey setting is persisted in TOML format
**And** the setting will be loaded on next application launch

**Given** I have selected Vietnamese (VI) as my default mode
**When** the configuration is saved
**Then** the input mode preference is persisted
**And** VI mode will be active on next application launch

---

### Story 4.2: Configuration Loading on Launch

As a Vietnamese user,
I want my previous settings to be applied when I start VietIME,
So that I don't have to reconfigure everything each time.

**Acceptance Criteria:**
**Given** a config.toml file exists in %APPDATA%\VietIME\
**When** I launch VietIME.exe
**Then** the configuration is loaded during initialization
**And** the saved input mode is applied (VI or EN)
**And** the saved hotkey is registered for mode toggling

**Given** the configuration file exists
**When** the application loads preferences
**Then** memory footprint remains < 15MB (NFR-Performance-04)
**And** launch time stays within 300ms target (NFR-Performance-02)
**And** the tray icon reflects the loaded mode immediately

**Given** no config.toml file exists (first launch)
**When** I launch VietIME.exe
**Then** default settings are applied (VI mode, Ctrl+Shift hotkey)
**And** a new config.toml is created with defaults
**And** the application functions normally with smart defaults

**Given** the config.toml file is corrupted or invalid
**When** the application attempts to load configuration
**Then** an error is logged
**And** default settings are applied as fallback
**And** the user is notified of the configuration error

---

### Story 4.3: Graceful Error Recovery

As a Vietnamese user,
I want the application to recover from errors without losing my work,
So that temporary issues don't prevent me from typing Vietnamese.

**Acceptance Criteria:**
**Given** a recoverable error occurs (e.g., hook installation failure)
**When** the error is detected
**Then** a CoreError or PlatformError is raised with context
**And** the error is logged for debugging
**And** the application attempts to recover (retry hook installation)

**Given** a critical error occurs that requires restart
**When** the error cannot be recovered
**Then** the user is notified with a clear error message
**And** the application offers to restart automatically
**And** current configuration is saved before restart

**Given** the application restarts after an error
**When** the application relaunches
**Then** previous settings and mode are restored
**And** the error is logged for bug reporting
**And** normal operation resumes

**Given** memory usage exceeds 15MB threshold (NFR-Performance-04)
**When** the high memory condition is detected
**Then** a warning is logged
**And** unnecessary buffers are cleared
**And** the application continues operating within normal limits

---

### Story 4.4: GitHub Bug Reporting Workflow

As a Vietnamese user experiencing issues,
I want an easy way to report bugs to the development team,
So that problems can be identified and fixed in future releases.

**Acceptance Criteria:**
**Given** a non-recoverable error occurs
**When** the error dialog is displayed
**Then** the error message includes a "Report Bug" button
**And** clicking the button opens GitHub Issues in the browser
**And** error details are pre-filled in the issue template

**Given** I want to report a bug manually
**When** I access the "Help" or "About" menu
**Then** a "Report a Bug" option is available
**And** clicking it opens GitHub Issues with the issue template
**And** the template includes system information (Windows version, app version)

**Given** I am submitting a bug report
**When** the GitHub Issues page loads
**Then** the issue template includes sections for:
**And** "Description" - What happened
**And** "Steps to Reproduce" - How to trigger the bug
**And** "Expected Behavior" - What should have happened
**And** "System Information" - Windows version, VietIME version

**Given** the application encounters an error
**When** error information is collected
**Then** error details include: error type, stack trace if available
**And** the error log is saved locally for inclusion in bug reports
**And** user privacy is maintained (no typed content is logged)

---





