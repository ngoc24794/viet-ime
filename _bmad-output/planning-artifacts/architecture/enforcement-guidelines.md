# Enforcement Guidelines

**All AI Agents MUST:**

1. **Follow Rust naming conventions** (RFC 430)
   - Functions: `snake_case`
   - Types: `PascalCase`
   - Constants: `SCREAMING_SNAKE_CASE`

2. **Use Result<T, E> for error handling**
   - No `unwrap()` in production code
   - Use `?` for propagation
   - Use `context()` for error enrichment

3. **Maintain module visibility boundaries**
   - `core` crate: `pub(crate)` only
   - Platform crates: private to workspace
   - Access via public API surface

4. **Follow buffer management patterns**
   - Fixed capacity array (no heap allocation)
   - Reset on 2-second timeout
   - Reset on application context switch

5. **Implement PlatformHook trait for all platforms**
   - Consistent callback signature
   - Platform-specific optimizations allowed
   - Error types unified across platforms

6. **Write tests for all new functionality**
   - Unit tests for core logic
   - Integration tests for cross-cutting concerns
   - Performance tests for NFR compliance

**Pattern Enforcement:**

- Code review against this architecture document
- Clippy lints enabled and warnings fixed
- `cargo test` must pass before PR
- Performance benchmarks for critical path

---