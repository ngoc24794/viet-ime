# Executive Summary

VietIME is a cross-platform Vietnamese Input Method Editor (IME) designed to replace Unikey by enabling Vietnamese text input in applications that don't natively support it - particularly terminals, IDEs, and command-line tools. The application targets all Vietnamese users from developers to casual office workers, requiring a design that is universally simple while powerful enough for technical workflows.

### Project Vision

**Core Value Proposition:** Type Vietnamese text in ANY application on Windows, Linux, or macOS - without installation, configuration, or performance impact.

**Differentation from Unikey:**
- **Universal Compatibility:** Works in terminals, IDEs, and all applications where Unikey fails
- **Cross-Platform:** Single experience across Windows, Linux, and macOS
- **Zero-Friction Deployment:** Portable single-file executable, no installation required
- **Performance:** <5ms input latency ensures instant typing feel
- **Simplicity:** Works great out-of-box for all users regardless of technical literacy

**Technical Context (Migration from .NET to Rust):**
- Current codebase: .NET 8.0 (WPF for Windows, Avalonia for Linux/macOS)
- Migration target: Rust for performance, small binary size, and memory safety
- UI Framework: `slint` for cross-platform native performance
- Architecture: Modular workspace (core/, platforms/, ui/)

### Target Users

**Primary User Segments:**

1. **Developers & Technical Users**
   - **Context:** Daily work in terminals, IDEs, command-line tools
   - **Pain Points:** Unikey doesn't work in development environments
   - **Needs:** Keyboard-first interaction, minimal UI distraction, reliable terminal compatibility
   - **Tech Literacy:** High - familiar with system tray apps, keyboard shortcuts
   - **Devices:** Desktops and laptops, often multi-monitor setups

2. **Office Workers & General Users**
   - **Context:** Daily office work (Word, Excel, email), messaging, web browsing
   - **Pain Points:** Need Vietnamese typing in various applications
   - **Needs:** Simple, intuitive interface, clear status indication
   - **Tech Literacy:** Varying - from basic to intermediate
   - **Devices:** Both desktops and laptops, single and multi-monitor

**Usage Context:**
- **Frequency:** Daily work - core productivity tool
- **Platform Mix:** Windows primary (MVP), expanding to Linux and macOS
- **Workflow Integration:** Always-on system tray application, minimal active UI needed
- **Performance Expectation:** Instant response, no typing lag or interruption

### Key Design Challenges

1. **Invisibility Challenge**
   - Application runs in background as system tray utility
   - **UX Need:** Clear, unambiguous status feedback for VI vs EN mode
   - **Risk:** Users uncertain if application is working or which mode is active
   - **Consideration:** Visual indicator must be noticeable but not intrusive

2. **Universal Application Compatibility**
   - Must work seamlessly across ALL applications: terminals, IDEs, browsers, Office apps, legacy software
   - **UX Need:** Consistent typing experience regardless of target application
   - **Technical Note:** Clipboard-based output (Ctrl+V) for terminal compatibility may create slightly different interaction feel
   - **Risk:** Inconsistent experience across application types erodes user confidence

3. **Universal Usability**
   - Target audience spans from technical developers to casual office workers ("cho mọi người")
   - **UX Need:** Zero learning curve, smart defaults, minimal configuration
   - **Risk:** Too technical = alienates general users; too simple = lacks power features for developers
   - **Consideration:** Progressive disclosure - simple by default, powerful when needed

4. **Cross-Platform Consistency**
   - Windows, Linux, and macOS have different platform conventions and UI patterns
   - **UX Need:** Familiar experience across all platforms while respecting platform conventions
   - **Risk:** Platform-native feel on each OS vs consistent cross-platform experience
   - **Consideration:** Platform-specific adaptations (hotkeys, tray behavior) while maintaining core UX consistency

5. **Zero Interruption Workflow**
   - Users expect Vietnamese typing to "just work" without disrupting their normal workflow
   - **UX Need:** Invisible operation, instant response, no thought required
   - **Technical Note:** <5ms input latency (95th percentile) is NFR, UX must feel equally instant
   - **Consideration:** Mode switching and error handling must not break typing flow

### Design Opportunities

1. **System Tray Excellence**
   - Tray icon is the primary and often only user touchpoint
   - **Opportunity:** Create exceptional system tray experience that sets new standard for IME applications
   - **Directions:**
     - Crystal-clear mode indication (VI vs EN) at glance
     - Quick access to common actions via right-click context menu
     - Thoughtful tray icon states and animations
     - Platform-appropriate tray behavior while maintaining consistency

2. **Smart Defaults Philosophy**
   - Most users should never need configuration
   - **Opportunity:** Delight users with "it just works" experience from first launch
   - **Directions:**
     - Sensible default settings for most users
     - Automatic preferences based on detected environment
     - Graceful degradation if configuration needed
     - Clear, minimal settings UI when required

3. **Keyboard-First Interaction**
   - Power users (developers) prefer keyboard over mouse
   - **Opportunity:** Competitive advantage through superior keyboard workflow
   - **Directions:**
     - Comprehensive keyboard shortcuts for all common actions
     - Mode toggle via keyboard (Ctrl+Shift configurable)
     - Keyboard-accessible settings and configuration
     - Mnemonic, discoverable shortcuts

4. **Portable Simplicity**
   - Single-file deployment enables unique UX opportunity
   - **Opportunity:** Eliminate installation friction entirely
   - **Directions:**
     - Double-click to run - no installation wizard
     - Auto-start option (optional, off by default for privacy)
     - Easy distribution - share file, it works
     - No upgrade hassles - replace file, new version

5. **Status Communication Excellence**
   - Users need instant confidence that Vietnamese mode is active
   - **Opportunity:** Create best-in-class mode indication that becomes signature feature
   - **Directions:**
     - Multiple feedback channels: tray icon, optional overlay, sound
     - Unambiguous visual distinction between VI and EN modes
     - Customizable notification preferences
     - Platform-appropriate notification methods
