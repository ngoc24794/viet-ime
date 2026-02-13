//! # Input Engine Module
//!
//! Defines the [`InputEngine`] trait and implements various input methods
//! for Vietnamese text entry (Telex, VNI).

pub mod telex;

// pub mod vni; // Future story

use std::fmt;

/// Result of processing a keystroke
///
/// # Fields
///
/// * `handled` - true if the key was processed by the engine (original key should be blocked)
/// * `output_text` - Optional text to send to the application
/// * `backspace_count` - Number of backspaces to send before output_text
/// * `current_buffer` - Current buffer state after processing
#[derive(Debug, Clone, PartialEq)]
pub struct ProcessKeyResult {
    /// true = block original key, false = let key through
    pub handled: bool,

    /// Text to send (if any)
    pub output_text: Option<String>,

    /// Number of backspaces to send before output_text
    pub backspace_count: usize,

    /// Buffer state after processing
    pub current_buffer: String,
}

impl ProcessKeyResult {
    /// Create a new ProcessKeyResult with default values
    pub fn new() -> Self {
        Self {
            handled: false,
            output_text: None,
            backspace_count: 0,
            current_buffer: String::new(),
        }
    }

    /// Create a result that passes the key through unchanged
    pub fn pass_through(buffer: String) -> Self {
        Self {
            handled: false,
            output_text: None,
            backspace_count: 0,
            current_buffer: buffer,
        }
    }

    /// Create a result that replaces the last characters
    pub fn replace(backspaces: usize, output: String, buffer: String) -> Self {
        Self {
            handled: true,
            output_text: Some(output),
            backspace_count: backspaces,
            current_buffer: buffer,
        }
    }
}

impl Default for ProcessKeyResult {
    fn default() -> Self {
        Self::new()
    }
}

/// Trait for Vietnamese input engines
///
/// # Example
///
/// ```rust,no_run
/// use vietime_core::engines::{InputEngine, ProcessKeyResult};
///
/// struct MyEngine;
///
/// impl InputEngine for MyEngine {
///     fn name(&self) -> &str {
///         "MyEngine"
///     }
///
///     fn process_key(&mut self, key: char, is_shift_pressed: bool) -> ProcessKeyResult {
///         // Process key here
///         ProcessKeyResult::new()
///     }
///
///     fn reset(&mut self) {
///         // Clear buffer
///     }
///
///     fn process_backspace(&mut self) -> bool {
///         // Handle backspace, return true if buffer was modified
///         false
///     }
///
///     fn get_buffer(&self) -> String {
///         String::new()
///     }
/// }
/// ```
pub trait InputEngine: Send + Sync {
    /// Get the engine name (e.g., "Telex", "VNI")
    fn name(&self) -> &str;

    /// Process a keystroke
    ///
    /// # Arguments
    ///
    /// * `key` - The character key that was pressed
    /// * `is_shift_pressed` - Whether Shift key was held down
    ///
    /// # Returns
    ///
    /// A [`ProcessKeyResult`] indicating what should be done
    fn process_key(&mut self, key: char, is_shift_pressed: bool) -> ProcessKeyResult;

    /// Reset the internal buffer (called on context switch, space, enter, etc.)
    fn reset(&mut self);

    /// Process backspace key
    ///
    /// # Returns
    ///
    /// true if buffer was modified, false if key should pass through
    fn process_backspace(&mut self) -> bool;

    /// Get current buffer state
    fn get_buffer(&self) -> String;

    /// Get buffer as a slice (for efficient inspection)
    fn get_buffer_slice(&self) -> &[char] {
        &[]
    }
}

impl fmt::Display for dyn InputEngine {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.name())
    }
}
