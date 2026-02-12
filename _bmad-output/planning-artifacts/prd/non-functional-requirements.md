# Non-Functional Requirements

### Performance

**NFR-Performance-01:** System shall process keystrokes and output Vietnamese characters within 5ms of keypress
- Acceptance: 95th percentile < 5ms, 99th percentile < 10ms
- Tested with rapid typing (15+ chars/second)

**NFR-Performance-02:** System shall achieve ready state within 300ms for 95% of launches across Windows 10/11 environments
- Acceptance: Ready state = tray icon visible + keyboard hook installed
- Tested on cold start (no OS cache) and warm start

**NFR-Performance-03:** System shall process consecutive keystrokes at 15+ characters/second with zero dropped keystrokes
- Acceptance: Tested with 30 seconds continuous typing
- Buffer timeout: 2 seconds of inactivity triggers reset

**NFR-Performance-04:** System shall maintain CPU usage < 1% when idle and memory footprint < 15MB during normal operation
- Acceptance: Idle state = no input processing for > 5 seconds
- Memory measured as private working set

### Scalability

**NFR-Scalability-01:** System shall support automatic updates for 5,000+ users without degraded performance
- Acceptance: Update check throttled to once per 24 hours
- GitHub Releases API used (no infrastructure required)
