# Implementation Patterns & Consistency Rules

> **Step 5 Note:** This step defines implementation patterns to ensure multiple AI agents write compatible, consistent code for the Rust desktop application migration.

### Pattern Categories Defined

**Critical Conflict Points Identified:**
- 6 areas where AI agents could make different implementation choices

### Naming Conventions

**Decision: Standard Rust Naming Conventions (RFC 430)**

| Category | Convention | Example |
|-----------|------------|----------|
| **Functions/Variables** | `snake_case` | `process_key()`, `current_buffer` |
| **Types/Structs** | `PascalCase` | `VietnameseChar`, `InputEngine` |
| **Constants** | `SCREAMING_SNAKE_CASE` | `MAX_BUFFER_SIZE`, `BUFFER_TIMEOUT_MS` |
| **Traits** | `PascalCase` | `PlatformHook`, `InputEngine` |
| **Macros** | `snake_case!` | `generate_windows_hook!()` |
| **Acronyms** | Treat as words | `HtmlParser` not `HTMLParser` |

**Domain-Specific Naming (Vietnamese Input):**

| Domain | Convention | Examples |
|---------|------------|-----------|
| **Engine Classes** | `<Method>Engine` | `TelexEngine`, `VniEngine` |
| **Tone Marks** | `Tone::<Name>` | `Tone::Acute`, `Tone::Grave`, `Tone::Hook` |
| **Vowel Types** | `Vowel::<Description>` | `Vowel::Basic`, `Vowel::WithDieresis` |
| **Special Chars** | Descriptive naming | `DIERESIS_O`, `HOOK_ABOVE` |

**Rationale:** Standard Rust conventions ensure:
- IDE autocomplete works correctly
- Code is readable by any Rust developer
- Consistent with ecosystem (crates.io, docs.rs)
- Clear distinction between types and values

---

### Module Organization

**Decision: Cargo Workspace with Public Crate Visibility**

```
vietime-rust/
├── Cargo.toml              # Workspace root
├── core/                   # Public crate: vietime-core
│   ├── Cargo.toml
│   └── src/
│       ├── lib.rs         # Public re-exports
│       ├── engines/        # Public module
│       │   ├── mod.rs
│       │   ├── telex.rs
│       │   └── vni.rs
│       └── models/         # Public module
│           ├── mod.rs
│           ├── vowel.rs
│           └── tone.rs
├── platforms/              # Private crates
│   ├── windows/        # crate: platform-windows
│   │   ├── Cargo.toml
│   │   └── src/
│   │       ├── lib.rs
│   │       └── hook.rs
│   ├── macos/          # Future: platform-macos
│   └── linux/          # Future: platform-linux
└── ui/                     # Public crate: vietime-ui
    ├── Cargo.toml
    └── src/
        ├── lib.rs         # Slint components
        └── main.rs        # Entry point
```

**Visibility Rules:**

| Module | Visibility | Rationale |
|--------|------------|-----------|
| `core` | `pub(crate)` | Shared by all workspace crates |
| `platforms/*` | Private | OS-specific, no cross-platform access |
| `ui` | `pub(crate)` | Shared by application crate |
| `engines/*` | `pub(super)` | Used by core, exposed via lib.rs |

---

### Error Handling Patterns

**Decision: Result<T, E> with Domain-Specific Error Types**

```rust
// Core error type
#[derive(Debug)]
pub enum CoreError {
    #[error("Invalid vowel sequence: {0}")]
    InvalidVowelSequence(String),

    #[error("Tone mark placement failed: {0}")]
    TonePlacementFailed(String),

    #[error("Buffer overflow: {0} > {1}")]
    BufferOverflow { current: usize, max: usize },
}

// Platform error type
#[derive(Debug)]
pub enum PlatformError {
    #[error("Hook installation failed: {0}")]
    HookInstallFailed(String),

    #[error("Text output failed: {0}")]
    TextOutputFailed(String),
}

// Unified result type
pub type Result<T> = std::result::Result<T, Box<dyn Error + Send + Sync>>;
```

**Error Handling Patterns:**

| Pattern | Usage | Example |
|---------|-------|----------|
| **Early return with ?** | Propagate errors immediately | `let ch = get_char(key)?;` |
| **Match on Result** | Handle errors explicitly | `match result { Ok(v) => ..., Err(e) => ... }` |
| **Context for errors** | Add context with .context() | `Err(e).context("hook callback")` |
| **Conversion with ?** | Convert error types | `platform_fn().map_err(CoreError::from)?` |

---

### Buffer Management Patterns

**Decision: Zero-Allocation Buffer with Fixed Capacity**

```rust
pub struct InputBuffer {
    chars: [char; MAX_BUFFER_SIZE],
    length: usize,
    last_activity: Instant,
}

impl InputBuffer {
    pub fn new() -> Self {
        Self {
            chars: ['\0'; MAX_BUFFER_SIZE],
            length: 0,
            last_activity: Instant::now(),
        }
    }

    pub fn push(&mut self, c: char) {
        if self.length < MAX_BUFFER_SIZE {
            self.chars[self.length] = c;
            self.length += 1;
            self.last_activity = Instant::now();
        }
    }

    pub fn reset(&mut self) {
        self.length = 0;
        self.chars = ['\0'; MAX_BUFFER_SIZE];
    }

    pub fn should_reset(&self) -> bool {
        self.last_activity.elapsed() > BUFFER_TIMEOUT
    }
}
```

**Buffer Constants:**

| Constant | Value | Rationale |
|----------|-------|-----------|
| `MAX_BUFFER_SIZE` | 20 | Longest Vietnamese word (e.g., "nghiênguênhchungbêyn") |
| `BUFFER_TIMEOUT` | 2 sec | Reset on inactivity per NFR |

---

### Platform Abstraction Patterns

**Decision: PlatformHook Trait with Async Callback**

```rust
pub trait PlatformHook {
    // Install hook with callback
    fn install(&mut self, callback: Box<dyn Fn(KeyEvent) -> bool>) -> Result<()>;

    // Uninstall hook
    fn uninstall(&mut self) -> Result<()>;

    // Send text to active window
    fn send_text(&self, text: &str) -> Result<()>;

    // Get current platform name
    fn platform_name(&self) -> &str;
}

// KeyEvent for cross-platform representation
#[derive(Clone, Debug)]
pub struct KeyEvent {
    pub key: VirtualKey,
    pub is_shift_pressed: bool,
    pub is_ctrl_pressed: bool,
}
```

**Implementation Requirements:**

| Platform | Hook Method | Output Method |
|----------|-------------|--------------|
| **Windows** | `SetWindowsHookEx(WH_KEYBOARD_LL)` | `SendInput(W)` or clipboard |
| **macOS** | `CGEvent.tapCreateCallback()` | `CGEvent.keyboard_set_event()` or clipboard |
| **Linux** | `evdev` device read | `uinput` device write or clipboard |

---

### Testing Strategy

**Decision: Unit Tests per Module + Integration Tests**

```
core/
├── src/
├── tests/
│   ├── engines/
│   │   ├── telex_tests.rs    # Telex engine tests
│   │   └── vni_tests.rs      # VNI engine tests
│   └── models/
│       ├── vowel_tests.rs     # Tone placement tests
│       └── tone_tests.rs      # Vowel rules tests
└── Cargo.toml    # [[bin]] for test binaries

platforms/windows/
├── src/
└── tests/
    └── hook_tests.rs           # Win32 hook integration tests

ui/
├── src/
└── tests/
    └── ui_tests.rs             # Slint component tests
```

**Testing Categories:**

| Category | Focus | Example |
|----------|-------|----------|
| **Unit Tests** | Single function behavior | `assert_eq!(telex.process('s'), Tone::Acute)` |
| **Integration Tests** | Cross-component | Engine + Hook integration |
| **Performance Tests** | NFR compliance | `assert!(process_time < Duration::from_millis(5))` |
| **Edge Case Tests** | User-reported bugs | Backspace in complex vowel sequences |

---
