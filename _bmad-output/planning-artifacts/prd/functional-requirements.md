# Functional Requirements

### Input Processing
- **FR1:** User can type Vietnamese characters using Telex input method
- **FR2:** System can process tone marks (s, f, r, x, j) and place correctly on vowels
- **FR3:** System can process special characters (w, [, ]) to create ơ, ư, đ
- **FR4:** System can handle backspace to undo previous keystrokes within current buffer

### Mode Management
- **FR5:** User can toggle between Vietnamese (VI) and English (EN) input modes
- **FR6:** User can identify current input mode via system tray icon indicator
- **FR7:** System can maintain input mode state across application switches

### Application Lifecycle
- **FR8:** User can launch application by double-clicking portable EXE without installation
- **FR9:** System can initialize and become ready for input within 300ms of launch
- **FR10:** User can terminate application via system tray context menu

### Text Output
- **FR11:** System can output processed Vietnamese text to active application cursor position
- **FR12:** System can send text via clipboard simulation (Ctrl+V) for terminal compatibility

### Configuration
- **FR13:** System can save user preferences (input mode, hotkey settings) to persistent storage
- **FR14:** System can load user preferences on application launch

### System Integration
- **FR15:** User can see application indicator in Windows system tray
- **FR16:** User can access context menu via right-click on system tray icon
- **FR17:** User can toggle input modes via keyboard shortcut (Ctrl+Shift)

### Error Handling
- **FR18:** System can recover from input errors via application restart
- **FR19:** User can report bugs via GitHub Issues workflow

---
