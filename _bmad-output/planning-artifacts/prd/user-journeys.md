# User Journeys

### Journey 1: Minh - The Vibe Coder (First-time User)

**Persona:**
- Full-stack developer using Claude Code (AI pair programmer) on Warp terminal
- Wants to add Vietnamese comments to code for team collaboration
- Pain point: Cannot type Vietnamese in terminal despite Unikey working fine elsewhere

**Opening Scene:**
Minh is in flow state. Claude Code just generated a complex data processing function, and he wants to add Vietnamese comments so teammates understand. He types `// Hàm này xử lý...` and then discovers - **he cannot type Vietnamese.** Unikey activity light is green, but Warp terminal still outputs `console.log("Xin chao")` - no diacritics.

**Rising Action:**
- Tests Unikey in Chrome - works fine. Tests in Word - works fine. Only terminal doesn't work.
- Searches Google: *"Unikey không gõ được trong terminal"*
- Finds StackOverflow thread from 2019 - "Terminal doesn't support Unicode input"
- Searches: *"gõ tiếng Việt trong Warp terminal"*
- **Boom!** VietIME appears - "Bộ gõ tiếng Việt cho Windows - hoạt động trong Terminal, PowerShell, Claude Code, Warp"
- Clicks GitHub repo. Reads: "Portable, không cần cài đặt", "Single file < 5MB"
- Downloads. Double-clicks `VietIME.exe`.

**Climax:**
System tray icon appears with **V** symbol. He returns to Warp terminal, tests: `taoj` → `tạo`.
```
"Ahaaaa... gõ được tiếng Việt rồi nè!!!!!!!"
```

**Resolution:**
Minh completes the Vietnamese prompt. Flow state returns. He opens Slack, shares with teammates: *"Mọi người ơi, tìm được công cụ gõ tiếng Việt cho terminal rồi!"*

From that day, **VietIME becomes Minh's default tool** whenever he codes.

---

### Journey 2: Lan - The Converted Unikey User (Daily User)

**Persona:**
- React developer, 5-year Unikey veteran
- Recently encountered same terminal typing limitation
- Skeptical about switching from familiar tools

**Opening Scene:**
Lan is coding a React app in VS Code. She wants to type a Vietnamese comment: `// Xử lý case khi user chưa verify email`

She types in VS Code Terminal: `// Xu ly case khi user...` - stops. Unikey not working. This is the **first time she feels Unikey is "inadequate."**

**Rising Action:**
- Opens Slack team channel - sees Minh's post: *"Tìm được tool gõ tiếng Việt cho terminal rồi!"*
- Clicks VietIME GitHub link
- **Reads description**: "Hỗ trợ Telex, VNI - toggle Ctrl+Shift"
- **Concern**: "App mới... không biết dùng có quen không? Telex giống Unikey không?"
- Messages Minh: *"Telex gõ giống Unikey không? Tập quen lâu."*
- Minh replies: *"Y chang! s=f, j=r, w=ơ, [ =ư. Thử đi!"*

**Climax:**
Lan downloads VietIME. Double-clicks. Tray icon V appears.

She opens VS Code Terminal, tests:
```
// xu ly case khi user -> backspace -> xu lý -> s -> lý
// xu ly case khi user chua verify email -> j -> chửaa
```

She stares at screen:
```
"Dùng không khác gì Unikey/EVKey... nhưng lại gõ được tiếng Việt trên Claude Code/Warp terminal"
```

**Resolution:**
Lan switches from Unikey to VietIME that same day. She realizes: **Same experience, but works where Unikey can't reach.**

---

### Journey 3: Tú - The Config Tinkerer (Advanced User)

**Persona:**
- Full-stack developer, power user
- Recently migrated from VS Code to Neovim
- Wants to customize everything - not satisfied with defaults

**Opening Scene:**
Tú has **3 problems** with default VietIME settings:
1. **Hotkey conflict**: `Ctrl+Shift` toggles VietIME... but it's also his Neovim buffer switch keybind
2. **Custom macro needed**: Always types `console.error("Debug:")` when debugging - wants `deg` → `console.error("Debug:")`
3. **Config in GUI**: Hates clicking through menus. Wants to edit TOML/YAML like other dev tools.

He opens VietIME settings window, clicks through... **"Làm sao để config đây?"**

**Rising Action:**
- Searches: *"VietIME config file custom macro"*
- Finds docs: **VietIME supports file-based config via `vietime.toml`**
- **Advanced features section**:
  ```toml
  [hotkeys]
  toggle = "Ctrl+Alt+V"  # Custom toggle

  [macros]
  deg = "console.error(\"Debug:\")"
  ```
- Edits `vietime.toml` in Neovim
- Restarts VietIME...

**Climax:**
Tests:
- Press `Ctrl+Alt+V` → **VietIME icon turns blue** (enabled)
- Press again → icon turns gray (disabled)
- Type `deg` in terminal → **`console.error("Debug:")`** appears

```
"Found the perfect config."
```

**Resolution:**
Tú submits feature request on GitHub: *"Add macro placeholder support for {cursor}"*

He becomes an **active contributor** - submits PRs for new features, writes blog post: *"Why VietIME Rust is the best Vietnamese input method for power users."*

---

### Journey 4: Nam - The Troubleshooter (Error Recovery)

**Persona:**
- QA engineer
- Tests edge cases that others don't anticipate
- Systematic bug reporter

**Opening Scene:**
Nam is testing VietIME. Thinks: *"Để xem input method này handle edge cases như thế nào."*

Opens Notepad, tests:
- Normal: `tieecs` → `tiệcs` ✅
- Fast typing: `xinchaoban` → `xin chào ban` ✅
- Backspace: `tieecs` → backspace → `tiec` → `s` → `tiệc` ✅

Then tests **edge case**:
```
Type: hoa asynchronous -> hoa asynchronnous -> backspace 3x -> hoa asyn -> c -> hoa async
```

**Rising Action:**
Expected: `hoa async`
Actual: `hoa asy̆nc` → **Wrong output!**

```
"Interesting... bug detected."
```

Retests:
- `tieecs` → `tiệcs` ✅
- `hoa asynchronous` → `hoa asynchronnous` → backspace ×3 → `hoa asyn` → `c` → **`hoa asy̆nc`** ❌

**Climax:**
Identifies bug pattern:
- When buffer has `syn` + backspace ×3 + `c` → wrong tone placement
- **Root cause**: Backspace count calculation wrong with complex vowel sequences

Restarts VietIME... bug persists. Not a random glitch.

**Resolution:**
Opens GitHub Issues, submits detailed bug report:

```
## Bug: Wrong tone placement after backspace in complex vowel sequences

**Steps to reproduce:**
1. Type: hoa asynchronous
2. Backspace ×3
3. Type: c

**Expected:** hoa async
**Actual:** hoa asy̆nc

**Environment:** Windows 11, VietIME Rust v0.1.0
```

Dev replies: *"Nice catch! Fixing in v0.1.1"*

---

### Journey 5: Huy - The Migrator (Contributor)

**Persona:**
- Rust developer, OSS contributor
- Scours GitHub for interesting Rust projects
- Wants to study architecture and contribute

**Opening Scene:**
Huy is browsing GitHub trending. Sees new repo: **"vietime-rust"** - 500+ stars in 2 weeks.

```
"Vietnamese input method... written in Rust? Interesting..."
```

Clicks repo:
- **Language**: Rust 🦀
- **Description**: "Migration from .NET to Rust - 100% rewrite"
- **Tech stack**: `windows-rs` for Win32 API, `slint` for UI

```
"Architecture này đáng để study."
```

**Rising Action:**
Clones repo, explores structure:
```
src/
├── core/           # Telex/VNI engines - pure Rust, no unsafe
├── hook/           # Win32 keyboard hook via windows-rs
└── ui/             # Slint-based tray application
```

Reads `core/engines/telex.rs`:
- **Clean Rust idioms**: `match`, `Option`, `Result`
- **No unsafe blocks** in business logic
- **Comprehensive unit tests**

```
"Core engine rất solid. Nhưng UI... "
```

Opens `ui/mainwindow.slint`:
```rust
export component MainWindow inherits Window {
    // Simple tray icon, minimal settings
    // TODO: Add proper settings panel
}
```

```
"UI còn sơ khai. Tôi có thể improve cái này!"
```

**Climax:**
Explores `ui/` directory:
- Settings panel hardcoded in Rust
- No live preview when changing hotkey
- Not responsive on different DPI settings

Opens Issues, sees: *"Request: Better settings UI with live preview"*

Architects solution:
- Migrate settings UI to Slint declarative UI
- Add two-way binding for live preview
- Support DPI scaling

```
"Architecture này hợp lý. Core và Hook tách biệt, UI có thể swap dễ dàng."
```

**Resolution:**
Forks repo, creates branch: `feature/settings-ui-redesign`

2 weeks later, submits PR:
- **608 lines added**, 124 deleted
- Slint-based settings panel with live preview
- DPI-aware rendering

Dev review: *"Clean implementation! Merging soon."*

---

### Journey 6: Thảo - System Tray User (The Minimalist)

**Persona:**
- Product Manager
- Needs to type Vietnamese across multiple applications daily
- Never touches technical settings - wants it to "just work"

**Opening Scene:**
A typical workday for Thảo:
- 9:00AM: Writing product requirements in Notion
- 10:30AM: Chatting with clients via Slack
- 2:00PM: Writing technical docs in VS Code
- 4:00PM: Emailing partners via Outlook

She toggles VI/EN **dozens of times**. Never opens Unikey settings.

**Rising Action:**
Receives notification from IT: *"Vấn đề với Unikey compatibility trên Windows 11, pls consider alternative."*

Thinks: *"Unikey vẫn work tốt mà..."*

Afternoon, opens VS Code to write docs. Types:
```
// Tính năng này cho phép user...
```

Unikey indicator green... but output: `// Tinh nang nay cho phep user...`

Restarts Unikey. Still doesn't work. **"Hỏng rồi."**

**Climax:**
Opens Slack, asks team. Dev lead replies:
```
"Cài VietIME đi. Dùng y chang Unikey, mượt mà hơn."
```

Downloads, double-clicks. Tray icon V appears.

Tests:
- **Notion**: `xin chao` + s → `xin chào` ✅
- **Slack**: `cam on` + f → `cảm ơn` ✅
- **VS Code**: `tai lieu` + j → `tài liệu` ✅
- **Outlook**: `trang kinh` + r → `trang kinh` ✅

**"Ctrl+Shift toggle cũng y chang."**

**Resolution:**
Thảo never opens VietIME settings. **"Cài đặt mặc định đã đủ dùng."**

Deletes Unikey.

---

### Journey Requirements Summary

| Journey | Key Requirements |
|---------|------------------|
| **Minh** (First-time) | Zero-install, instant readiness, terminal compatibility |
| **Lan** (Converted) | Familiar UX (Telex), cross-editor support |
| **Tú** (Power) | Custom hotkeys, file-based config, macros |
| **Nam** (Troubleshooter) | Error recovery, bug reporting workflow |
| **Huy** (Contributor) | Clean architecture, modular UI, contribution flow |
| **Thảo** (Minimalist) | Default-first, cross-app, familiar UX |

---
