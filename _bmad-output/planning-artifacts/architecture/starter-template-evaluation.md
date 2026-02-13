# Starter Template Evaluation

> **Step 3 Note:** This is a Rust desktop application migration from an existing .NET codebase, not a greenfield web project requiring starter templates. The technical stack and architecture have already been established in PRD and user journey documents.

### Primary Technology Domain

**Desktop Application (Native Windows → Cross-platform Vision)**

Based on project requirements and migration context from .NET to Rust, this is a native desktop application project with:
- Real-time keyboard interception at OS level
- Cross-platform architecture design for future macOS/Linux support
- System tray UI with minimal settings interface
- Performance-critical input processing (< 5ms latency)

### Tech Stack Already Defined

**From PRD (Journey 5 - Huy - Contributor):**

| Component | Technology | Rationale |
|------------|-----------|-----------|
| **Language** | Rust | Performance, small binary size, memory safety |
| **Win32 API** | `windows-rs` crate | Official Microsoft Rust bindings for Win32 API |
| **UI Framework** | `slint` | Cross-platform UI, native performance |
| **Architecture** | Modular workspace | `core/`, `platforms/`, `ui/` |

### Rust Ecosystem Analysis

**Key Crates to Evaluate:**

| Category | Crate Options | Notes |
|-----------|---------------|-------|
| **Win32 API** | `windows-rs` (official), `winapi` (legacy) | PRD specifies `windows-rs` |
| **UI Framework** | `slint`, `egui`, `tauri` | PRD specifies `slint` for tray app |
| **Clipboard** | `clipboard-win`, `arboard` | Terminal-compatible output required |
| **Config** | `serde`, `toml` | TOML-based config specified |
| **Hot-reload** | `notify`, `debounce` | Post-MVP feature for config changes |

### Note: Migration Strategy

Rather than using a starter template, this project will:
1. Migrate existing .NET components to Rust
2. Maintain established architectural patterns
3. Follow Rust idioms and best practices
4. Preserve business logic (Telex/VNI engines, tone rules)

---
