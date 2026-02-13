---
name: bash-windows
description: 'Run bash commands on Windows by automatically converting Windows paths to WSL/Git Bash compatible format'
---

# Bash Windows Skill

When the user asks to run a bash command on Windows (e.g., `cd C:\Users\...`, `cargo test` in a directory), you MUST:

## Path Conversion Rules

Convert Windows paths to bash-compatible format:

**For WSL (Windows Subsystem for Linux):**
- `C:\Users\...` → `/mnt/c/Users/...`
- `D:\path\to\...` → `/mnt/d/path/to/...`

**For Git Bash (MinGW):**
- `C:\Users\...` → `/c/Users/...`
- `D:\path\to\...` → `/d/path/to/...`

**Current working directory:**
- `C:\Users\ngocnv\source\repos\viet-ime` → `/mnt/c/Users/ngocnv/source/repos/viet-ime` (WSL)
- Or use `$PWD` environment variable which bash automatically resolves

## Detection

Check if the command contains:
- Windows drive letters: `C:\`, `D:\`, etc.
- Backslashes in paths

## Implementation

When running bash commands:

1. **Replace `cd <windows-path>` with direct path:**
   ```
   # Instead of: cd C:\Users\ngocnv\source\repos\viet-ime\core && cargo test
   # Use: cd /mnt/c/Users/ngocnv/source/repos/viet-ime/core && cargo test
   ```

2. **Or use $PWD for current directory:**
   ```
   # Instead of: cd C:\Users\ngocnv\source\repos\viet-ime\core && cargo test
   # Use: cd core && cargo test  (if already in project root)
   ```

3. **Replace backslashes with forward slashes:**
   ```
   # C:\Users\path\to\file → /mnt/c/Users/path/to/file
   ```

## Special Cases

- If path is already in bash format (`/mnt/c/...` or `/c/...`), keep as-is
- If using relative paths from current directory, no conversion needed
- For `cargo test`, `npm test`, etc., prefer running from project root with relative paths

## Examples

| Original Command | Converted Command (WSL) | Converted Command (Git Bash) |
|-----------------|------------------------|------------------------------|
| `cd C:\Users\ngocnv\source\repos\viet-ime\core && cargo test` | `cd /mnt/c/Users/ngocnv/source/repos/viet-ime/core && cargo test` | `cd /c/Users/ngocnv/source/repos/viet-ime/core && cargo test` |
| `cd core && cargo test` (from repo root) | `cd core && cargo test` | `cd core && cargo test` |
| `cat C:\Users\file.txt` | `cat /mnt/c/Users/file.txt` | `cat /c/Users/file.txt` |
