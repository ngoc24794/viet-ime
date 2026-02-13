//! # VietIME Core Library
//!
//! Core Vietnamese input method engine supporting Telex and VNI input methods.
//! This library provides the input processing engine that can be integrated
//! with platform-specific keyboard hooks.
//!
//! ## Architecture
//!
//! - [`engines`] - Input engine implementations (Telex, VNI)
//! - [`models`] - Vietnamese character models and mappings
//!
//! ## Usage
//!
//! ```rust,no_run
//! use vietime_core::engines::telex::TelexEngine;
//! use vietime_core::engines::InputEngine;
//!
//! let mut engine = TelexEngine::new();
//! let result = engine.process_key('a', false);
//! ```

pub mod engines;
pub mod models;

/// Core engine exports
pub use engines::{InputEngine, ProcessKeyResult};

// Re-export commonly used types
pub use models::vietnamese::{ToneIndex, VietnameseChar};

// Re-export vietnamese module for doctests
pub use models::vietnamese;

/// Version information
pub const VERSION: &str = env!("CARGO_PKG_VERSION");

/// Maximum buffer size for Vietnamese word processing
/// Longest Vietnamese words typically don't exceed 15 characters
/// 20 provides safety margin for compound words
pub const MAX_BUFFER_SIZE: usize = 20;

/// Auto-reset timeout for buffer in milliseconds
/// If no keystroke occurs within 2 seconds, buffer is cleared
pub const BUFFER_TIMEOUT_MS: u64 = 2000;
