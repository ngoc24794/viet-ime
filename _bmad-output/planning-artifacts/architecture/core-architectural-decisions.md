# Core Architectural Decisions

> **Step 4 Note:** The detailed web-app architectural decision categories (Data, Authentication, API, Frontend, Infrastructure) do not apply to this native Rust desktop application. Core architectural decisions have already been established in the PRD and migration requirements.

### Pre-Defined Architectural Decisions (from PRD)

**Critical Decisions (Blocking Implementation):**

| Decision | Choice | Rationale |
|-----------|-------|-----------|
| **Language** | Rust | Performance, memory safety, small binary size |
| **Win32 API** | `windows-rs` crate | Official Microsoft Rust bindings for Win32 |
| **UI Framework** | `slint` | Cross-platform, native performance |
| **Architecture** | Modular workspace | `core/`, `platforms/`, `ui/` |
| **Output Method** | Clipboard-based (Ctrl+V) | Terminal compatibility |

**Important Decisions (Shape Architecture):**

| Decision | Choice | Rationale |
|-----------|-------|-----------|
| **Platform Abstraction** | `PlatformHook` trait | Consistent behavior cross-platform |
| **Config Format** | TOML | Cross-platform compatibility |
| **Config Location** | `%APPDATA%\VietIME\` | Windows standard |
| **Update Source** | GitHub Releases API | No infrastructure required |

**Deferred Decisions (Post-MVP):**

| Decision | Timeline | Rationale |
|-----------|-----------|-----------|
| **Auto-update** | Post-MVP | Complexity, can be added later |
| **Custom macros** | Post-MVP | Feature enhancement |
| **File-based config** | Post-MVP | Use in-memory settings initially |

---
