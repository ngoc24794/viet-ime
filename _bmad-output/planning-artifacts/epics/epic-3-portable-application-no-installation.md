# Epic 3: Portable Application - No Installation

Users can run the application immediately by double-clicking a portable EXE - no installation required, no restart needed

**FRs covered:** FR8, FR9, FR10

### Story 3.1: Cargo Workspace Structure Setup

As a developer building VietIME,
I want a modular Cargo workspace with core/, platforms/, and ui/ components,
So that the codebase is maintainable and supports future cross-platform expansion.

**Acceptance Criteria:**
**Given** I am setting up the project structure
**When** I create the Cargo.toml workspace root
**Then** the workspace includes three member crates: core, platforms/windows, ui
**And** the core crate is public (vietime-core)
**And** the platforms crate is private (platform-specific)
**And** the ui crate is public (vietime-ui)

**Given** the workspace is created
**When** I review the directory structure
**Then** core/ contains engines/ (Telex, VNI) and models/ (Vowel, Tone)
**And** platforms/windows/ contains Win32 API hook implementation
**And** ui/ contains Slint UI components
**And** visibility rules follow RFC 430 (snake_case, PascalCase)

**Given** the workspace build configuration
**When** I run `cargo build --release`
**Then** a single executable is produced
**And** no external runtime dependencies are required
**And** the binary size is optimized for portability

---

### Story 3.2: Application Launch & Initialization

As a Vietnamese user,
I want to launch VietIME by double-clicking the EXE file,
So that I can start typing Vietnamese immediately without installation.

**Acceptance Criteria:**
**Given** I have the VietIME.exe file
**When** I double-click the executable
**Then** the application launches without installation wizard
**And** the system tray icon appears within 300ms (NFR-Performance-02)
**And** the keyboard hook is installed on startup
**And** Vietnamese mode is active by default

**Given** the application is launching
**When** initialization is in progress
**Then** the application enters ready state when tray icon is visible
**And** the keyboard hook is successfully installed
**And** CPU usage remains < 1% when idle (NFR-Performance-04)

**Given** Windows SmartScreen warning may appear
**When** the application is first run
**Then** the user can proceed despite the warning
**And** the application functions normally after approval
**And** no code signing is required (MVP limitation)

**Given** the application is ready
**When** I begin typing Vietnamese text
**Then** the input engine is active and processing keystrokes
**And** the mode indicator shows VI (#2563EB) in the system tray

---

### Story 3.3: Application Termination

As a Vietnamese user,
I want to exit VietIME through the system tray menu,
So that the application closes cleanly and releases all resources.

**Acceptance Criteria:**
**Given** VietIME is running
**When** I right-click the system tray icon
**Then** the context menu appears with an "Exit" option
**And** clicking "Exit" terminates the application

**Given** I select "Exit" from the context menu
**When** the application terminates
**Then** the keyboard hook is uninstalled
**And** the system tray icon is removed
**And** all resources are released gracefully
**And** no background processes remain running

**Given** the application is terminating
**When** shutdown is in progress
**Then** user preferences are saved to persistent storage
**And** the current mode state is preserved for next launch
**And** no crash or error occurs during shutdown

**Given** the application has been terminated
**When** I check running processes
**Then** no VietIME processes are active
**And** the system tray icon is no longer visible

---
