# UX Consistency Patterns

### Button Hierarchy

| Button Type | Visual Style                    | Usage Context          |
|-------------|---------------------------------|----------------------|
| Primary     | Solid background (#2563EB), white text, 600 weight | Save, OK, Apply       |
| Secondary   | Transparent background, gray text, 400 weight | Cancel, Close         |
| Destructive | Solid background (#EF4444), white text, red border | Exit, Delete, Clear    |

**Usage Guidelines:**
- **Save = Primary:** In settings, "Save" (or Apply via close) is primary action
- **OK = Primary:** In dialogs, "OK" is primary action
- **Cancel = Secondary:** Always available for escape

### Form Interaction

**Specification:**
```
Form Layout:
┌───────────────────────────────────┐
│  Label                         │
│  ┌──────────┐                  │
│  │ Input    │                  │
│  └──────────┘                  │
├───────────────────────────────────┤
│  [Help: What's Telex?]      │
│                               │
└───────────────────────────────────┘

Spacing: 8px between label and input
```

**Input States:**

```
Normal (Focus):          Error (Invalid):         Success (Valid):
┌──────────┐            ┌───────────┐          ┌───────────┐          ┌───────────┘
│ [Telex]   │            │ [Telex]    │          │ ✓ [Telex]   │
│  ________ │            │  _________│          │  _________│
└──────────┘            └───────────┘          └───────────┘

Border: 1px #E2E8F0   Border: 2px #EF4444         Border: 2px #10B981
```

**Validation Rules:**
1. **Real-time Validation:** Immediate feedback on input
2. **Error Messages:** Clear, action-oriented ("Invalid hotkey")
3. **Success Feedback:** Visual confirmation on valid input

### Feedback & Notification

**Specification:**

| Feedback Type  | Visual Style                     | Duration     | Position           |
|---------------|----------------------------------|-------------|------------------|
| Success        | Green background (#10B981), brief | 2000ms      | Near action button  |
| Error          | Orange background (#F59E0B), clear  | 3000ms      | Near input field    |
| Warning        | Yellow background (#F59E0B), persistent | Until dismissed | Top of window       |
| Info (optional) | Blue background (#2563EB), tooltip   | 3000ms      | Mouse hover        |

**Feedback Levels:**
1. **Success:** Subtle, reinforcing (green background, brief display)
2. **Error:** Attention-grabbing (orange background, clear message)
3. **Warning:** Noticeable (yellow background, persistent until action)
4. **Info:** Optional (blue background, brief tooltip)

### Navigation

**Purpose:** Consistent, predictable navigation

**Specification:**

```
Tab Navigation (Settings):
┌─────────────────────────────────────────┐
│  [Input Method] [Encoding] [Advanced] │  Active tab gets darker underline
├──────────────────────────────────────────┤
│                                       │
│  Tab 1 Content (Input Method)         │
│  ┌──────────────────────────────────┐  │
│  │ Radio options (vertically stacked)│
│  └──────────────────────────────────┘ │
│                                       │
│  [Help: What's Telex?]            │
└───────────────────────────────────┘

Keyboard Navigation:
- Tab: Next tab
- Shift+Tab: Previous tab
- Arrow keys: Navigate within tab content
- Enter: Activate focused element
```

**Navigation Rules:**
1. **Tab Order:** Logical flow (Input → Encoding → Advanced)
2. **Default Tab:** Input Method always active when settings opened
3. **Focus Management:** Tab key moves focus, click already focuses
4. **Visual Indication:** Active tab clearly marked with underline/color

### Modal & Dialog Behavior

**Specification:**

```
Modal Behavior (Settings Window):
┌───────────────────────────────────┐
│  ×                              │  Close button (top-right)
│  ┌────────────────────────────┐ │
│  │ VietIME Settings            │  Title bar
│  ├────────────────────────────┤ │
│  │ [Input Method] [Encoding]    │  Tab navigation
│  │ [Advanced]        │
│  └────────────────────────────┘ │
│  ──────────────────────────────────│
│  ┌──────────────────────────────┐ │
│  │  [Apply] [Cancel]           │  Action buttons (bottom-right)
│  └──────────────────────────────┘ │
│  ──────────────────────────────────│

Behavior: Non-modal (can interact with main app)
```

**Window Behavior:**
1. **Close on Apply:** Save/Apply closes window
2. **Close on Outside:** Click outside closes window
3. **Escape to Cancel:** ESC key acts like Cancel button
4. **Non-blocking:** Main app still works when settings open

### Loading & Empty States

**Specification:**

```
Loading State:
┌───────────────────────────────────────┐
│  Loading configuration...            │  Spinner or progress indicator
│  ⏳ Please wait                   │  Centered, minimal UI
│                                     │  Background: #F8FAFC
│                                     │  Duration: Until ready

Empty State (Settings):
┌───────────────────────────────────────┐
│  No custom hotkeys configured         │  Centered in container
│                                     │ Subtle hint text
│  + Add custom hotkey                 │  Action to populate
└───────────────────────────────────────┘
```

**Loading Guidelines:**
- **Timeout:** 30 seconds max before showing error
- **Progress Indicator:** Subtle animation or text
- **User Feedback:** User knows system is working

**Empty State Guidelines:**
- **Clear Message:** "No items" or "Add custom hotkey"
- **Action Available:** Button to add item
- **Subtle Styling:** Gray text, minimal visual weight
