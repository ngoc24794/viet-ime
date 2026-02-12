# Product Scope

### MVP - Minimum Viable Product

**Goal:** Fully functional Vietnamese input method for Windows that replaces the .NET version.

**In Scope:**
- **Core Engine:** Telex and VNI input processing with complete tone placement rules
- **Hook Layer:** Windows keyboard hook via Win32 API (SetWindowsHookEx)
- **UI:** System tray icon with context menu (Toggle VI/EN, Settings, Exit)
- **Configuration:** Save/load settings (engine type, hotkey, output method)
- **Output Method:** Clipboard-based (Ctrl+V) for terminal compatibility

**Out of Scope (Post-MVP):**
- Auto-update mechanism
- File-based configuration editing
- Custom macro/text expansion
- Cross-platform support (macOS, Linux)

---

### Growth Features (Post-MVP)

**Competitive Enhancements:**
- **Auto-update:** Silent update mechanism for seamless improvements
- **File-based config:** Edit settings via TOML/YAML configuration file
- **Custom macros:** User-defined text expansion and custom key sequences

---

### Vision (Future)

**Dream Version:**
- **Cross-platform:** Unified codebase supporting Windows, macOS, and Linux
- **Community ecosystem:** Plugin system for custom input methods beyond Telex/VNI
- **Developer-first:** API/SDK for integrating Vietnamese input into other applications

---
