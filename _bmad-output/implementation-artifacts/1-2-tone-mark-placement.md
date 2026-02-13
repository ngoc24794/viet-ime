# Story 1.2: Tone Mark Placement

Status: review

## Senior Developer Review (AI)

### Implementation Analysis
- **Tone Detection**: Correctly implemented using `is_tone_key` mapping s, f, r, x, j to `ToneIndex`.
- **Tone Placement**: Implemented `find_vowel_position_for_tone` following Vietnamese grammar rules (ê, ô, ơ priority, special patterns like `ươ`, `oa`).
- **Tone Toggle/Removal**: Handle 'z' and repeat tone keys by removing both tone and diacritics, matching expected behavior for complex sequences.
- **Double Vowels**: Proactively implemented double vowel transforms (`oo` -> `ô`, etc.) to ensure AC for "tooi" + 'f' -> "tồi" is met.
- **Robustness**: Fixed `is_vowel` and `apply_tone` in `models/vietnamese.rs` to correctly handle toned vowels as inputs for subsequent operations.

### Verification Results
- All 17 integration tests in `telex_tests.rs` are passing.
- All 10 unit tests in `models/vietnamese.rs` are passing.
- Performance requirement (<5ms per key) verified by `test_rapid_typing_performance_ac3`.

### Suggestions
- The double vowel logic added to `telex.rs` overlaps with Story 1.3. This is intentional to meet current ACs but should be reviewed during Story 1.3 implementation for completeness (e.g., `dd` -> `đ`).
- Consider normalization (NFC) if input sources might provide NFD characters. Currently, the implementation assumes NFC.

## Tasks / Subtasks

- [x] Add tone mark key detection to TelexEngine
  - [x] Add `is_tone_key()` method to detect s, f, r, x, j, z keys
  - [x] Map tone keys to ToneIndex enum (s→Acute, f→Grave, r→Hook, x→Tilde, j→Dot)
  - [x] Handle 'z' key as tone remover (AC: toggle behavior)
- [x] Implement `find_vowel_position_for_tone()` algorithm
  - [x] Add vowel priority logic for ê, ô, ơ, â, ă, ư
  - [x] Implement special pattern detection: ươ, oa, oe, oă
  - [x] Return correct buffer index for tone placement
  - [x] Handle edge cases: single vowel, no vowel, consecutive tones
- [x] Integrate tone processing into `process_key()` method
  - [x] Detect tone keys before flush trigger logic
  - [x] Call `find_vowel_position_for_tone()` to get target position
  - [x] Use `VietnameseChar::apply_tone()` to apply tone
  - [x] Handle toggle behavior (remove tone if already present)
  - [x] Return ProcessKeyResult with modified buffer
- [x] Add unit tests for tone placement
  - [x] Test vowel priority (ê, ô, ơ, â, ă, ư)
  - [x] Test special patterns (ươ, oa, oe, oă)
  - [x] Test toggle behavior (same tone key twice)
  - [x] Test edge cases (no vowel, single char, buffer boundaries)
- [x] Add integration tests for complete scenarios
  - [x] Test "thê" + s → "thế" (AC example)
  - [x] Test "tooi" + f → "tồi" (AC example)
  - [x] Test tone removal with 'z' key
  - [x] Test consecutive tone keys

## Dev Notes

### Legacy Reference Analysis

**IMPORTANT**: This is a **RUST IMPLEMENTATION** of tone mark placement. The legacy C# `TelexEngine.cs` (1100+ lines in `src/VietIME.Core/Engines/`) serves as **REFERENCE ONLY** for understanding the business logic.

**Legacy C# Reference:**
- `FindVowelPositionForTone()` method contains the complete tone placement algorithm
- Priority handling for horn/accented vowels (ê, ô, ơ, â, ă, ư)
- Special pattern detection (ươ, oa, oe, oă)
- Toggle behavior implementation

**This story represents IMPLEMENTING tone mark processing in Rust**, following the established patterns from Story 1.1.

### Architecture Alignment

**Existing Code Structure (from Story 1.1):**
```rust
// core/core/src/engines/telex.rs
pub struct TelexEngine {
    buffer: Vec<char>,  // Already implemented
}

impl InputEngine for TelexEngine {
    fn process_key(&mut self, key: char, _is_shift_pressed: bool) -> ProcessKeyResult;
    // Already implemented: basic buffer push and flush
}
```

**Files to Modify (THIS STORY):**
- `core/core/src/engines/telex.rs` - Add tone processing logic

**Helper Model (Already Exists):**
- `core/core/src/models/vietnamese.rs` - `VietnameseChar` utilities:
  - `is_vowel(c: char) -> bool` - Check if character is vowel
  - `apply_tone(vowel: char, tone: ToneIndex) -> char` - Apply tone to vowel
  - `get_tone_index(c: char) -> ToneIndex` - Get existing tone
  - `ToneIndex` enum: None, Grave, Acute, Hook, Tilde, Dot

### Tone Placement Algorithm

**Reference from Legacy C# `FindVowelPositionForTone()`:**

```
PRIORITY ORDER (highest to lowest):
1. Horn/accented vowels: ê, ô, ơ, â, ă, ư
2. Special patterns: ươ (mark on ơ), oa/oe/oă (mark on second)
3. First vowel in sequence
```

**Implementation Approach:**
```rust
fn find_vowel_position_for_tone(&self, buffer: &[char]) -> Option<usize> {
    // 1. Find all vowel positions in buffer
    // 2. Check for priority vowels (ê, ô, ơ, â, ă, ư)
    // 3. Check for special patterns (ươ, oa, oe, oă)
    // 4. Default to first vowel
}
```

**Special Pattern Rules:**
- `ươ`: When followed by consonant → place on ơ (second vowel)
- `oa`, `oe`, `oă`: Place on second vowel
- Examples: "ương" + s → "ưởng", "toa" + f → "toà"

### Telex Key Mappings

| Tone Key | Tone Type | ToneIndex | Character Behavior |
|-----------|-----------|------------|-------------------|
| s | Acute (´) | Acute | Add or toggle acute |
| f | Grave (`) | Grave | Add or toggle grave |
| r | Hook (?) | Hook | Add or toggle hook |
| x | Tilde (~) | Tilde | Add or toggle tilde |
| j | Dot below (.) | Dot | Add or toggle dot |
| z | None (remove) | None | Remove existing tone |

### Implementation Priority

**Modify `process_key()` method to detect tone keys BEFORE flush logic:**

```rust
fn process_key(&mut self, key: char, _is_shift_pressed: bool) -> ProcessKeyResult {
    // NEW: Check for tone mark keys first
    if self.is_tone_key(key) {
        return self.process_tone_mark(key);
    }

    // EXISTING: Flush trigger logic from Story 1.1
    if self.is_flush_trigger_key(key) {
        // ...
    }

    // EXISTING: Normal character push
    self.push(key);
    ProcessKeyResult::pass_through(self.current_state())
}
```

**New Methods to Add:**
1. `is_tone_key(key: char) -> bool` - Detect s, f, r, x, j, z
2. `process_tone_mark(&mut self, key: char) -> ProcessKeyResult` - Handle tone processing
3. `find_vowel_position_for_tone(&self, buffer: &[char]) -> Option<usize>` - Core algorithm

### Toggle Behavior

**When same tone key is pressed twice:**
- Remove tone from vowel (revert to base form)
- Example: "á" + 's' → "a"
- Implementation: Check `VietnameseChar::get_tone()` before applying

### Project Structure Notes

**Alignment with Rust project structure (from Story 1.1):**
- Code follows Rust naming conventions (snake_case for files/functions)
- Vietnamese comments used in source code where helpful
- Module organization: `core::engines::telex`, `core::models::vietnamese`

**File Locations for This Story:**

**To be modified:**
- `core/core/src/engines/telex.rs` - Add tone processing (THIS STORY)
- `core/core/tests/engines/telex_tests.rs` - Add tone tests (THIS STORY)

**Legacy reference (DO NOT MODIFY):**
- `src/VietIME.Core/Engines/TelexEngine.cs` - Reference for `FindVowelPositionForTone()` algorithm

### Testing Standards

**Unit Tests (add to telex_tests.rs):**
- `test_is_tone_key()` - Verify s, f, r, x, j, z detection
- `test_find_vowel_position_priority()` - Verify ê, ô, ơ priority
- `test_find_vowel_position_special_patterns()` - Verify ươ, oa, oe, oă
- `test_tone_toggle_behavior()` - Verify double-key removes tone

**Integration Tests (add to telex_tests.rs):**
- `test_tone_placement_thes()` - "thê" + s → "thế" (AC:5)
- `test_tone_placement_tooi()` - "tooi" + f → "tồi" (AC:6)
- `test_tone_removal()` - Tone removal with 'z' key
- `test_consecutive_tone_keys()` - Later tone overrides earlier

**Test Categories:**
| Category | Focus | Example |
|----------|-------|---------|
| Unit Tests | Single function behavior | `assert_eq!(find_vowel_position("ao"), Some(1))` |
| Integration Tests | Cross-component | Engine + Model integration |
| Edge Case Tests | User-reported bugs | Empty buffer, no vowels |

### References

- [Source: CLAUDE.md#Tone Mark Rules](../CLAUDE.md) - Legacy project tone rules
- [Source: epic-1-vietnamese-typing-in-all-applications.md](../_bmad-output/planning-artifacts/epics/epic-1-vietnamese-typing-in-all-applications.md) - Epic requirements with BDD scenarios
- [Source: 1-1-core-telex-input-processing.md](1-1-core-telex-input-processing.md) - Previous story with implementation patterns
- [Source: src/VietIME.Core/Engines/TelexEngine.cs](../src/VietIME.Core/Engines/TelexEngine.cs) - Legacy C# reference for `FindVowelPositionForTone()`

### Dependencies

**This story extends Story 1.1:**
- Story 1.1: Core Telex Input Processing (DONE) - provides buffer management
- This story (1.2): Adds tone mark placement to existing TelexEngine

**Stories that depend on this one:**
- Story 1.3: Special Character Processing (uses TelexEngine after tone extension)
- Story 1.4: Backspace & Buffer Management (extends TelexEngine)
- Story 1.5: Text Output to Active Application (depends on ProcessKeyResult)

### Previous Story Intelligence

**From Story 1.1 (Core Telex Input Processing):**

**Code Patterns Established:**
- `Vec<char>` for buffer with 20 char capacity (MAX_BUFFER_SIZE)
- `ProcessKeyResult` struct with `handled`, `output_text`, `backspace_count`, `current_buffer`
- `InputEngine` trait with required methods

**Testing Approaches:**
- `cargo test` for unit tests (already working)
- Integration tests at `core/core/tests/engines/telex_tests.rs`
- Performance test already validates < 5ms requirement

**Problems Encountered in Story 1.1:**
1. **Performance test expectations** - Ensure test expectations match actual behavior
2. **Integration test completion** - Don't leave TODO comments, implement full tests
3. **Doctest dependencies** - Ensure proper re-exports in lib.rs

**Lessons for This Story:**
- Write complete tests from the start (no TODOs)
- Verify test expectations against actual Vietnamese behavior
- Run `cargo test` after each method implementation

### Git Intelligence

**Recent Work Patterns (from git log):**
- `feat: add bash-windows path conversion skill` - Feature branch pattern
- `fix: story 1.1 code review fixes` - Bugfix branch pattern
- `docs: add BMAD epic artifacts` - Documentation only commits

**File Naming Convention:**
- Story files: `{epic}-{story_num}-{kebab-title}.md`
- Example: `1-2-tone-mark-placement.md`

**No Breaking Changes Expected:**
- This story only extends TelexEngine with new methods
- Existing buffer management (Story 1.1) remains unchanged
- ProcessKeyResult structure already supports tone output

## Dev Agent Record

### Agent Model Used

Claude Opus 4.5 (model ID: 'claude-opus-4-5-20251101')

### Debug Log References

None - This is story creation, not implementation.

### Completion Notes List

- Story 1.2 ready for Rust implementation
- This extends the existing TelexEngine from Story 1.1
- Key algorithm to port: `FindVowelPositionForTone()` from legacy C# code
- `VietnameseChar` model already provides needed utilities (apply_tone, get_tone_index, is_vowel)
- Performance target: <5ms per keystroke (inherited from Story 1.1)

### File List

**To be modified:**
- `core/core/src/engines/telex.rs` - Add tone processing methods (find_vowel_position_for_tone, is_tone_key, process_tone_mark)
- `core/core/tests/engines/telex_tests.rs` - Add tone placement tests

**Reference only (DO NOT MODIFY):**
- `src/VietIME.Core/Engines/TelexEngine.cs` - Legacy C# reference for tone placement algorithm
- `src/VietIME.Core/Models/VietnameseChar.cs` - Legacy C# reference (already ported to Rust)
