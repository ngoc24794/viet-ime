# Epic 1: Vietnamese Typing in All Applications

Users can type Vietnamese text using Telex input method in ANY application - including terminals and IDEs where Unikey doesn't work

**FRs covered:** FR1, FR2, FR3, FR4, FR11, FR12

### Story 1.1: Core Telex Input Processing

As a Vietnamese typist,
I want to type basic Vietnamese characters using Telex keystrokes (a, s, d, e, o, w, etc.),
So that I can compose Vietnamese text character by character.

**Acceptance Criteria:**
**Given** the Vietnamese input mode is active
**When** I type a letter key (a-z)
**Then** the character is added to the input buffer
**And** the buffer state is tracked internally
**And** no visible output occurs until processing is complete

**Given** the input buffer contains characters
**When** I type a consonant or non-modifying key
**Then** the buffer is processed and output
**And** the buffer is cleared for the next sequence

**Given** rapid typing (15+ chars/second)
**When** consecutive keystrokes are processed
**Then** no keystrokes are dropped
**And** processing time is < 5ms per keystroke (95th percentile)

---

### Story 1.2: Tone Mark Placement

As a Vietnamese typist,
I want to type tone marks (s, f, r, x, j) and have them correctly placed on vowels,
So that I can write properly accented Vietnamese words.

**Acceptance Criteria:**
**Given** the input buffer contains a vowel sequence
**When** I type a tone mark key (s, f, r, x, j)
**Then** the tone is applied to the correct vowel
**And** vowels with accent/circumflex (ê, ô, ơ, â, ă, ư) have priority
**And** special pattern `ươ` places mark on `ơ` when followed by consonant
**And** patterns `oa`, `oe`, `oă` place mark on second vowel

**Given** the input buffer contains "thê"
**When** I type 's' (acute tone)
**Then** the output is "thế"
**And** the tone appears on the 'ê' character

**Given** the input buffer contains "tooi"
**When** I type 'f' (grave tone)
**Then** the output is "tồi"
**And** the tone appears on the second 'o' character

---

### Story 1.3: Special Character Processing

As a Vietnamese typist,
I want to type special characters (w, [, ]) to create ơ, ư, đ,
So that I can write Vietnamese words with these unique characters.

**Acceptance Criteria:**
**Given** the input buffer contains a vowel
**When** I type 'w' after 'o'
**Then** the vowel 'o' is converted to 'ơ'
**And** the buffer reflects the modified character

**Given** the input buffer contains a vowel
**When** I type 'w' after 'u' or 'o'
**Then** the vowel is converted to 'ư'
**And** the buffer reflects the modified character

**Given** the input buffer contains 'd'
**When** I type '[' (bracket)
**Then** the 'd' is converted to 'đ'
**And** the buffer reflects the modified character

**Given** the input buffer contains 'D'
**When** I type '[' (bracket)
**Then** the 'D' is converted to 'Đ'
**And** the buffer reflects the modified character with uppercase

---

### Story 1.4: Backspace & Buffer Management

As a Vietnamese typist,
I want to use backspace to undo previous keystrokes within the current buffer,
So that I can correct mistakes without losing my entire typed sequence.

**Acceptance Criteria:**
**Given** the input buffer contains processed keystrokes
**When** I press backspace
**Then** the most recent keystroke is removed from the buffer
**And** the remaining buffer state is maintained
**And** no output is generated until the buffer is complete or timeout occurs

**Given** the input buffer has been inactive for 2 seconds
**When** the timeout threshold is reached
**Then** the buffer is automatically reset
**And** the buffer capacity (MAX_BUFFER_SIZE = 20) is enforced
**And** zero-allocation design is maintained

**Given** the input buffer is empty
**When** I press backspace
**Then** the keystroke is passed through normally to the active application
**And** no buffer processing occurs

---

### Story 1.5: Text Output to Active Application

As a Vietnamese typist,
I want processed Vietnamese text to appear at my cursor position in any application,
So that I can use Vietnamese input in terminals, IDEs, and all applications.

**Acceptance Criteria:**
**Given** the input buffer contains a complete Vietnamese sequence
**When** the buffer is ready for output
**Then** backspaces are sent to clear the original keystrokes
**And** the processed Vietnamese text is copied to clipboard
**And** Ctrl+V is simulated to paste the text
**And** the cursor position is maintained

**Given** I am typing in a terminal application (e.g., Warp, VS Code terminal)
**When** Vietnamese text is output
**Then** the clipboard-based method (Ctrl+V) is used
**And** the text appears correctly in the terminal
**And** the output feels instant and natural

**Given** I am typing in a standard application
**When** Vietnamese text is output
**Then** the output appears at the active cursor position
**And** no duplicate keystrokes occur

---
