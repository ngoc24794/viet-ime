# Success Criteria

### User Success

**Core User Value:** Vietnamese developers can finally type Vietnamese in Terminal, Claude Code, Warp, and all applications where Unikey fails.

**Success Moments:**
- **Zero-friction setup:** User downloads single < 5MB EXE, double-clicks, and starts typing Vietnamese immediately - no installation, no .NET runtime required
- **Instant readiness:** App launches in < 300ms from double-click - ready to type
- **Terminal typing works:** User types `tieecs` → `tiệcs` successfully in VS Code Terminal, PowerShell, Claude Code, Warp - no lag, no missed keystrokes
- **Effortless switching:** User toggles VI/EN modes with Ctrl+Shift or double-click tray icon - instant feedback

**Measurable Outcomes:**
| Metric | Target |
|--------|--------|
| Binary size | < 5MB single portable EXE |
| Startup time | < 300ms (double-click → ready) |
| Input latency | < 5ms (keypress → character appears) |
| Zero dropped keystrokes | When typing 15+ chars/second continuously |
| Installation friction | None - portable, double-click to run |

---

### Business Success

**3-Month Success:** 100+ active Vietnamese developers using VietIME Rust daily for terminal-based development work

**12-Month Success:** Replace Unikey/EVKey as preferred Vietnamese input method - 5,000+ active users, top 3 search result for "bộ gõ tiếng Việt terminal"

**Measurable Outcomes:**
| Metric | 3-Month | 12-Month |
|--------|----------|-----------|
| Active Users | > 100 | > 5,000 |
| Retention (1-week) | > 70% | > 80% |
| DAU/MAU Ratio | > 60% | > 65% |
| Growth Rate | +20% MoM | +15% MoM |
| Search Ranking | Top 10 for relevant queries | Top 3 |

---

### Technical Success

**Performance Standards:** The Rust implementation sets new benchmarks for responsiveness and resource efficiency.

**Measurable Outcomes:**
| Metric | Target |
|--------|--------|
| Input latency | < 5ms |
| App startup | < 300ms |
| CPU usage (idle) | < 1% |
| Memory footprint | < 15MB |
| Crash rate | < 0.1% of sessions |
| Dropped keystrokes | 0% (even at rapid typing) |

---
