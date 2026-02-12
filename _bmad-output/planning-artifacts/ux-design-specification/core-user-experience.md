# Core User Experience

### Defining Experience

**Core Value Proposition:** Type Vietnamese text in ANY application - especially in terminals, IDEs, and command-line tools where Unikey fails.

**Primary User Action:** Typing Vietnamese text - fundamental action that defines product value. All other interactions (mode switching, configuration, updates) are secondary to this core experience.

**Success Definition:** Users experience success when Vietnamese text appears correctly in unsupported applications (terminals like Warp, IDEs like Claude Code/VS Code) on first try. The moment a user types "thế" and sees "thế" (with proper Vietnamese accents) in a terminal where Unikey never worked - that's the defining success moment that defines this product's value.

### Platform Strategy

**Platform Scope:** Desktop application only - Windows (MVP), Linux and macOS (future)

**Interaction Model:** Keyboard-first, mouse-minimal
- Primary interaction: Keyboard (typing, mode switching, shortcuts)
- Secondary interaction: Mouse (settings UI only when needed)
- Design for power users who prefer keyboard over mouse

**Technical Platform Requirements:**
- System tray integration (primary touchpoint and status indicator)
- Win32 API hook for Windows (keyboard interception at OS level)
- Cross-platform architecture for future macOS/Linux support
- Clipboard access for Ctrl+V simulation (terminal compatibility method)

**Connectivity:** Completely offline operation
- No internet required for core functionality
- Auto-update check optional (can be skipped)
- No cloud dependencies or API calls

**Platform Capabilities to Leverage:**
- System tray notifications (mode switch confirmation, errors)
- Keyboard hooks at OS level (universal application compatibility)
- Clipboard access (text output method for terminals)

### Effortless Interactions

**1. Mode Switching (VI ↔ EN)**
- **Current Struggle:** Users can't tell which mode is active with Unikey
- **Effortless Goal:** Single keyboard shortcut (Ctrl+Shift) with instant, unambiguous visual feedback
- **Design Consideration:** Tray icon change + optional overlay notification
- **Success Criteria:** Instant feedback, no interruption to typing flow

**2. Typing Vietnamese in Terminals**
- **Current Struggle:** Unikey doesn't work in terminals/IDEs at all
- **Effortless Goal:** Type Vietnamese text in any terminal without thought or effort
- **Design Consideration:** Clipboard-based output must feel instant and natural

**3. First Launch Experience**
- **Current Struggle:** Installation wizards, configuration dialogs, restarts required
- **Effortless Goal:** Double-click EXE, start typing Vietnamese immediately
- **Design Consideration:** No configuration needed for first-time success, smart defaults

**4. Mode Clarity**
- **Current Struggle:** Never sure if VI or EN mode is active
- **Effortless Goal:** Glance at system tray = instantly know current mode
- **Design Consideration:** Crystal-clear visual distinction between VI and EN states

**5. Application Detection**
- **Current Struggle:** No awareness of which app currently has focus
- **Effortless Goal:** Auto-detect if current app supports Vietnamese natively
- **Design Consideration:** Smart mode switching based on application context

### Critical Success Moments

**Make-or-Break User Flows:**

1. **First Launch Moment**
   - User double-clicks VietIME.exe
   - Application appears in system tray
   - User types Vietnamese - it works immediately
   - **Success Criteria:** No configuration, no restart, just works

2. **First Terminal Success**
   - User opens terminal (Warp, VS Code integrated terminal, etc.)
   - Types Vietnamese text
   - Sees proper accents output correctly
   - **Success Criteria:** Vietnamese text appears correctly where Unikey never worked

3. **Mode Switch Confirmation**
   - User presses Ctrl+Shift (or configured shortcut)
   - Visual feedback confirms mode change
   - User continues typing in new mode
   - **Success Criteria:** Instant feedback, no interruption to typing flow

4. **Full Sentence Completion**
   - User types complete Vietnamese sentence with complex tones
   - Every character converts correctly
   - No corrections needed
   - **Success Criteria:** Zero errors in full sentence, feels effortless

**Failure Modes to Prevent:**

1. **"Doesn't Work in Terminal"**
   - User tries to type in terminal
   - Vietnamese output doesn't appear or appears incorrectly
   - User thinks "this is just like Unikey"
   - **Prevention:** Clipboard-based output must be tested thoroughly for terminals

2. **"Stuck in Wrong Mode"**
   - User can't tell which mode is active
   - Types in wrong language, has to backspace and retry
   - Gets frustrated with mode ambiguity
   - **Prevention:** Unambiguous visual indicator in system tray

3. **"Typing Lag"**
   - User types, experiences delay before Vietnamese appears
   - Feels sluggish, breaks typing flow
   - **Prevention:** <5ms input latency must be met, UI must feel instant

### Experience Principles

**Guiding Principles for All UX Decisions:**

**1. Universal Compatibility First**
   - Core value proposition: "Type Vietnamese text anywhere"
   - If it doesn't work in terminals/IDEs, product has failed
   - Clipboard-based output method ensures universal application compatibility
   - This is differentiator - nail this, everything else follows

**2. Effortless Mode Clarity**
   - Switching between VI and EN modes must be completely effortless
   - Users struggle with not knowing which mode is active (current Unikey pain)
   - Visual feedback must be instant and unambiguous
   - Mode switching should never interrupt typing flow
   - Single keyboard shortcut (Ctrl+Shift) with immediate confirmation

**3. Zero-Friction Deployment**
   - No installation wizard - just run EXE
   - Works out-of-box without configuration for most users
   - First launch success: double-click → start typing Vietnamese immediately
   - Progressive disclosure: simple by default, powerful when needed
   - Portable simplicity: share file, it works
   - No upgrade hassles - replace file, new version

**4. Terminal Excellence**
   - Terminals and IDEs are critical use case (primary user pain point)
   - First-time success in terminal is make-or-break moment
   - Vietnamese output in terminals must work flawlessly
   - This is where users realize "this is better than Unikey"
   - Terminal experience defines product reputation

**5. Silent Operation**
   - Application should feel invisible during normal typing
   - Auto-detect if current app supports Vietnamese natively (smart defaults)
   - No typing lag, no interruption, no thought required
   - Only visible when needed (mode indication, tray icon, notifications)
   - When it works perfectly, users forget it's running
