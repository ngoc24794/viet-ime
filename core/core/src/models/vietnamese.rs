//! # Vietnamese Character Model
//!
//! Provides character mappings and utilities for Vietnamese text processing.
//!
//! ## Features
//!
//! - Vowel/tone mappings for all Vietnamese characters
//! - Character transformation utilities (a->ă, a->â, etc.)
//! - Tone detection and application
//! - Vowel identification

use phf::{phf_map, Map};
use std::sync::OnceLock;

/// Vietnamese tone marks
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum ToneIndex {
    /// No tone (nguyen)
    None = 0,
    /// Grave accent (huyền `)
    Grave = 1,
    /// Acute accent (sác ´)
    Acute = 2,
    /// Hook above (hỏi ?)
    Hook = 3,
    /// Tilde (ngã ~)
    Tilde = 4,
    /// Dot below (nặng .)
    Dot = 5,
}

impl ToneIndex {
    /// Check if this is a tone (not None)
    pub fn is_toned(self) -> bool {
        self != Self::None
    }

    /// Get the Telex key for this tone
    pub fn telex_key(self) -> Option<char> {
        match self {
            Self::Acute => Some('s'),
            Self::Grave => Some('f'),
            Self::Hook => Some('r'),
            Self::Tilde => Some('x'),
            Self::Dot => Some('j'),
            Self::None => Some('z'), // z removes tone
        }
    }
}

/// Vowel map: base vowel -> all tone variants
///
/// Maps each base vowel to an array of its 6 variants:
/// [none, grave, acute, hook, tilde, dot]
///
/// Example: 'a' -> ['a', 'à', 'á', 'ả', 'ã', 'ạ']
fn vowel_map() -> &'static Map<char, [char; 6]> {
    static MAP: OnceLock<Map<char, [char; 6]>> = OnceLock::new();
    MAP.get_or_init(|| {
        phf_map! {
            // a -> à á ả ã ạ
            'a' => ['a', 'à', 'á', 'ả', 'ã', 'ạ'],
            'A' => ['A', 'À', 'Á', 'Ả', 'Ã', 'Ạ'],

            // ă -> ằ ắ ẳ ẵ ặ
            'ă' => ['ă', 'ằ', 'ắ', 'ẳ', 'ẵ', 'ặ'],
            'Ă' => ['Ă', 'Ằ', 'Ắ', 'Ẳ', 'Ẵ', 'Ặ'],

            // â -> ầ ấ ẩ ẫ ậ
            'â' => ['â', 'ầ', 'ấ', 'ẩ', 'ẫ', 'ậ'],
            'Â' => ['Â', 'Ầ', 'Ấ', 'Ẩ', 'Ẫ', 'Ậ'],

            // e -> è é ẻ ẽ ẹ
            'e' => ['e', 'è', 'é', 'ẻ', 'ẽ', 'ẹ'],
            'E' => ['E', 'È', 'É', 'Ẻ', 'Ẽ', 'Ẹ'],

            // ê -> ề ế ể ễ ệ
            'ê' => ['ê', 'ề', 'ế', 'ể', 'ễ', 'ệ'],
            'Ê' => ['Ê', 'Ề', 'Ế', 'Ể', 'Ễ', 'Ệ'],

            // i -> ì í ỉ ĩ ị
            'i' => ['i', 'ì', 'í', 'ỉ', 'ĩ', 'ị'],
            'I' => ['I', 'Ì', 'Í', 'Ỉ', 'Ĩ', 'Ị'],

            // o -> ò ó ỏ õ ọ
            'o' => ['o', 'ò', 'ó', 'ỏ', 'õ', 'ọ'],
            'O' => ['O', 'Ò', 'Ó', 'Ỏ', 'Õ', 'Ọ'],

            // ô -> ồ ố ổỗộ
            'ô' => ['ô', 'ồ', 'ố', 'ổ', 'ỗ', 'ộ'],
            'Ô' => ['Ô', 'Ồ', 'Ố', 'Ổ', 'Ỗ', 'Ộ'],

            // ơ -> ờ ớ ở ỡ ợ
            'ơ' => ['ơ', 'ờ', 'ớ', 'ở', 'ỡ', 'ợ'],
            'Ơ' => ['Ơ', 'Ờ', 'Ớ', 'Ở', 'Ỡ', 'Ợ'],

            // u -> ù ú ủ ũ ụ
            'u' => ['u', 'ù', 'ú', 'ủ', 'ũ', 'ụ'],
            'U' => ['U', 'Ù', 'Ú', 'Ủ', 'Ũ', 'Ụ'],

            // ư -> ừ ứ ửữự
            'ư' => ['ư', 'ừ', 'ứ', 'ử', 'ữ', 'ự'],
            'Ư' => ['Ư', 'Ừ', 'Ứ', 'Ử', 'Ữ', 'Ự'],

            // y -> ỳ ý ỷ ỹ ỵ
            'y' => ['y', 'ỳ', 'ý', 'ỷ', 'ỹ', 'ỵ'],
            'Y' => ['Y', 'Ỳ', 'Ý', 'Ỷ', 'Ỹ', 'Ỵ'],
        }
    })
}

/// Base vowel transformation map (horn -> base)
///
/// Maps horn/marked vowels back to their base form
fn base_vowel_map() -> &'static Map<char, char> {
    static MAP: OnceLock<Map<char, char>> = OnceLock::new();
    MAP.get_or_init(|| {
        phf_map! {
            'a' => 'a', 'ă' => 'a', 'â' => 'a',
            'A' => 'A', 'Ă' => 'A', 'Â' => 'A',
            'e' => 'e', 'ê' => 'e',
            'E' => 'E', 'Ê' => 'E',
            'o' => 'o', 'ô' => 'o', 'ơ' => 'o',
            'O' => 'O', 'Ô' => 'O', 'Ơ' => 'O',
            'u' => 'u', 'ư' => 'u',
            'U' => 'U', 'Ư' => 'U',
        }
    })
}

/// Vietnamese character utilities
pub struct VietnameseChar;

impl VietnameseChar {
    /// Check if character is a Vietnamese vowel
    ///
    /// # Examples
    ///
    /// ```
    /// assert!(vietime_core::VietnameseChar::is_vowel('a'));
    /// assert!(vietime_core::VietnameseChar::is_vowel('ă'));
    /// assert!(!vietime_core::VietnameseChar::is_vowel('d'));
    /// ```
    pub fn is_vowel(c: char) -> bool {
        let lower = c.to_ascii_lowercase();
        matches!(lower,
            'a' | 'ă' | 'â' | 'e' | 'ê' | 'i' | 'o' | 'ô' | 'ơ' | 'u' | 'ư' | 'y'
        )
    }

    /// Get the base vowel (without tone or horn)
    ///
    /// # Examples
    ///
    /// ```
    /// assert_eq!(vietime_core::VietnameseChar::get_base_vowel('ậ'), 'a');
    /// assert_eq!(vietime_core::VietnameseChar::get_base_vowel('Ê'), 'E');
    /// ```
    pub fn get_base_vowel(c: char) -> char {
        // First get vowel without tone (preserves horn)
        let without_tone = Self::get_vowel_without_tone(c);
        // Then remove horn if present
        base_vowel_map().get(&without_tone).copied().unwrap_or(without_tone)
    }

    /// Get vowel without tone (preserves horn)
    ///
    /// # Examples
    ///
    /// ```
    /// assert_eq!(vietime_core::VietnameseChar::get_vowel_without_tone('ậ'), 'â');
    /// assert_eq!(vietime_core::VietnameseChar::get_vowel_without_tone('ệ'), 'ê');
    /// ```
    pub fn get_vowel_without_tone(c: char) -> char {
        for (base, variants) in vowel_map().entries() {
            if variants.contains(&c) {
                return *base;
            }
        }
        c
    }

    /// Get the tone index of a character
    ///
    /// # Examples
    ///
    /// ```
    /// use vietime_core::vietnamese::ToneIndex;
    /// assert_eq!(vietime_core::VietnameseChar::get_tone_index('ạ'), ToneIndex::Dot);
    /// assert_eq!(vietime_core::VietnameseChar::get_tone_index('a'), ToneIndex::None);
    /// ```
    pub fn get_tone_index(c: char) -> ToneIndex {
        for (_base, variants) in vowel_map().entries() {
            if let Some(pos) = variants.iter().position(|&v| v == c) {
                return match pos {
                    0 => ToneIndex::None,
                    1 => ToneIndex::Grave,
                    2 => ToneIndex::Acute,
                    3 => ToneIndex::Hook,
                    4 => ToneIndex::Tilde,
                    5 => ToneIndex::Dot,
                    _ => ToneIndex::None,
                };
            }
        }
        ToneIndex::None
    }

    /// Apply a tone to a vowel
    ///
    /// # Examples
    ///
    /// ```
    /// use vietime_core::vietnamese::ToneIndex;
    /// assert_eq!(vietime_core::VietnameseChar::apply_tone('a', ToneIndex::Acute), 'á');
    /// ```
    pub fn apply_tone(vowel: char, tone: ToneIndex) -> char {
        if let Some(variants) = vowel_map().get(&vowel) {
            let index = tone as usize;
            if index < variants.len() {
                return variants[index];
            }
        }
        vowel
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_is_vowel() {
        assert!(VietnameseChar::is_vowel('a'));
        assert!(VietnameseChar::is_vowel('ă'));
        assert!(VietnameseChar::is_vowel('â'));
        assert!(VietnameseChar::is_vowel('e'));
        assert!(VietnameseChar::is_vowel('ê'));
        assert!(VietnameseChar::is_vowel('i'));
        assert!(VietnameseChar::is_vowel('o'));
        assert!(VietnameseChar::is_vowel('ô'));
        assert!(VietnameseChar::is_vowel('ơ'));
        assert!(VietnameseChar::is_vowel('u'));
        assert!(VietnameseChar::is_vowel('ư'));
        assert!(VietnameseChar::is_vowel('y'));
        assert!(!VietnameseChar::is_vowel('d'));
        assert!(!VietnameseChar::is_vowel('b'));
    }

    #[test]
    fn test_get_vowel_without_tone() {
        assert_eq!(VietnameseChar::get_vowel_without_tone('ậ'), 'â');
        assert_eq!(VietnameseChar::get_vowel_without_tone('ạ'), 'a');
        assert_eq!(VietnameseChar::get_vowel_without_tone('ệ'), 'ê');
        assert_eq!(VietnameseChar::get_vowel_without_tone('ò'), 'o');
        assert_eq!(VietnameseChar::get_vowel_without_tone('x'), 'x');
    }

    #[test]
    fn test_get_tone_index() {
        assert_eq!(VietnameseChar::get_tone_index('a'), ToneIndex::None);
        assert_eq!(VietnameseChar::get_tone_index('à'), ToneIndex::Grave);
        assert_eq!(VietnameseChar::get_tone_index('á'), ToneIndex::Acute);
        assert_eq!(VietnameseChar::get_tone_index('ả'), ToneIndex::Hook);
        assert_eq!(VietnameseChar::get_tone_index('ã'), ToneIndex::Tilde);
        assert_eq!(VietnameseChar::get_tone_index('ạ'), ToneIndex::Dot);
    }

    #[test]
    fn test_apply_tone() {
        assert_eq!(VietnameseChar::apply_tone('a', ToneIndex::None), 'a');
        assert_eq!(VietnameseChar::apply_tone('a', ToneIndex::Acute), 'á');
        assert_eq!(VietnameseChar::apply_tone('ă', ToneIndex::Grave), 'ằ');
        assert_eq!(VietnameseChar::apply_tone('â', ToneIndex::Dot), 'ậ');
        assert_eq!(VietnameseChar::apply_tone('e', ToneIndex::Tilde), 'ẽ');
        assert_eq!(VietnameseChar::apply_tone('ê', ToneIndex::Hook), 'ể');
    }
}
