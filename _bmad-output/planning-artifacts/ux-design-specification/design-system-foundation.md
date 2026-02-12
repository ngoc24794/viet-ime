# Design System Foundation

### Color System

**Primary Color Palette:**

| Color Name   | Hex     | Usage                    |
|-------------|--------|----------|
| Primary     | #2563EB  | Vietnamese Blue - Brand color |
| Secondary   | #64748B  | Slate Gray - Neutral      |
| Background Light | #FFFFFF  | Pure White               |
| Background Dark  | #1E293B  | Dark Slate             |
| Text Primary | #1E293B  | Near Black               |
| Text Secondary| #64748B | Medium Gray              |
| Border/Lines  | #E2E8F0  | Light Gray           |
| Success      | #10B981  | Green (subtle)        |
| Error        | #EF4444  | Red (clear)        |
| Warning      | #F59E0B  | Orange (noticeable)  |

**Semantic Color Mapping (Mode Indication):**
| State            | Hex     | Usage                    |
|---------------|--------|----------|
| VI Mode (Active) | #2563EB  | Vietnamese Blue - Brand color |
| EN Mode (Inactive)| #94A3B8  | Slate Gray - Neutral      |

### Typography System

**Typeface Selection:**
- Windows: Segoe UI
- macOS: San Francisco
- Linux: Inter or system default

**Type Scale:**

```
h1 (Page Title)    20px / 600 weight
h2 (Section Header)  16px / 600 weight
Body (UI Text)      14px / 400 weight
Small (Labels/Hints) 12px / 400 weight
Micro (Status)       11px / 400 weight
```

**Typography Rules:**
- **Line Height:** 1.5 for body, 1.2 for headers
- **Letter Spacing:** 0 for Vietnamese (avoid diacritic clipping)
- **Font Fallback:** System UI fonts with Unicode fallbacks

### Spacing & Layout Foundation

**Spacing System:**
```
Unit: 4px (industry standard)

xs  4px   - Icon padding, tight spacing
sm  12px  - Button padding, related items
md  12px  - Section spacing
lg  16px  - Component separation
xl  24px  - Page margins
```

**Layout Principles:**
1. **Compact Density** - System tray utility should feel lightweight
2. **Vertical Stack** - Settings use simple vertical layout
3. **Touch Targets** - Minimum 44px for mouse interaction
4. **Grid System** - Optional 4-column grid for settings
