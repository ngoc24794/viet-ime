# ═══════════════════════════════════════════════════════
# VietIME Installer for Windows
# irm https://raw.githubusercontent.com/donamvn/viet-ime/master/scripts/install.ps1 | iex
# ═══════════════════════════════════════════════════════

$ErrorActionPreference = "Stop"

$Repo = "donamvn/viet-ime"
$DownloadUrl = "https://github.com/$Repo/releases/latest/download/VietIME.exe"
$InstallDir = "$env:LOCALAPPDATA\VietIME"
$ExeName = "VietIME.exe"
$ExePath = "$InstallDir\$ExeName"

function Write-Color($Color, $Prefix, $Message) {
    Write-Host "[$Prefix] " -ForegroundColor $Color -NoNewline
    Write-Host $Message
}

function Info($msg)  { Write-Color Cyan    "VietIME" $msg }
function Ok($msg)    { Write-Color Green   "VietIME" $msg }
function Warn($msg)  { Write-Color Yellow  "VietIME" $msg }

Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor White
Write-Host "  VietIME — Bo go Tieng Viet" -ForegroundColor White
Write-Host "═══════════════════════════════════════" -ForegroundColor White
Write-Host ""

# ─── Kiem tra uninstall ───
if ($args -contains "--uninstall" -or $args -contains "uninstall") {
    Info "Go cai dat VietIME..."

    # Tat process
    Get-Process -Name "VietIME" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

    # Xoa file
    if (Test-Path $InstallDir) {
        Remove-Item -Recurse -Force $InstallDir
        Ok "Da xoa $InstallDir"
    }

    # Xoa shortcut Start Menu
    $StartMenu = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\VietIME.lnk"
    if (Test-Path $StartMenu) {
        Remove-Item -Force $StartMenu
        Ok "Da xoa shortcut Start Menu"
    }

    # Xoa shortcut Startup
    $Startup = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\VietIME.lnk"
    if (Test-Path $Startup) {
        Remove-Item -Force $Startup
        Ok "Da xoa shortcut Startup"
    }

    # Xoa khoi PATH
    $UserPath = [Environment]::GetEnvironmentVariable("Path", "User")
    if ($UserPath -and $UserPath.Contains($InstallDir)) {
        $NewPath = ($UserPath.Split(";") | Where-Object { $_ -ne $InstallDir }) -join ";"
        [Environment]::SetEnvironmentVariable("Path", $NewPath, "User")
        Ok "Da xoa khoi PATH"
    }

    Write-Host ""
    Ok "Da go cai dat VietIME!"
    Write-Host ""
    return
}

# ─── Cai dat ───

Info "He dieu hanh: Windows $([Environment]::OSVersion.Version.Major)"

# 1. Tao thu muc
if (-not (Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
}

# Tat process cu neu dang chay
Get-Process -Name "VietIME" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

# 2. Tai VietIME.exe
Info "Dang tai VietIME.exe..."
try {
    $ProgressPreference = 'SilentlyContinue'
    Invoke-WebRequest -Uri $DownloadUrl -OutFile $ExePath -UseBasicParsing
    $Size = [math]::Round((Get-Item $ExePath).Length / 1MB, 1)
    Ok "Tai xong (${Size} MB)"
} catch {
    Write-Host "[VietIME] " -ForegroundColor Red -NoNewline
    Write-Host "Khong the tai VietIME.exe: $_"
    return
}

# 3. Them vao PATH
$UserPath = [Environment]::GetEnvironmentVariable("Path", "User")
if (-not $UserPath -or -not $UserPath.Contains($InstallDir)) {
    if ($UserPath) {
        [Environment]::SetEnvironmentVariable("Path", "$UserPath;$InstallDir", "User")
    } else {
        [Environment]::SetEnvironmentVariable("Path", $InstallDir, "User")
    }
    $env:Path = "$env:Path;$InstallDir"
    Ok "Da them vao PATH: $InstallDir"
} else {
    Ok "PATH da co san"
}

# 4. Tao shortcut Start Menu
Info "Tao shortcut..."
try {
    $WshShell = New-Object -ComObject WScript.Shell

    # Start Menu
    $StartMenuPath = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\VietIME.lnk"
    $Shortcut = $WshShell.CreateShortcut($StartMenuPath)
    $Shortcut.TargetPath = $ExePath
    $Shortcut.WorkingDirectory = $InstallDir
    $Shortcut.Description = "VietIME - Bo go Tieng Viet"
    $Shortcut.Save()
    Ok "Shortcut Start Menu: OK"
} catch {
    Warn "Khong the tao shortcut: $_"
}

# 5. Hoi tu khoi dong cung Windows
$Startup = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\VietIME.lnk"
$AddStartup = $true

# Kiem tra neu chay interactive
if ([Environment]::UserInteractive -and $Host.Name -eq "ConsoleHost") {
    Write-Host ""
    $reply = Read-Host "Tu dong chay khi khoi dong Windows? (y/n)"
    if ($reply -notmatch "^[yY]") {
        $AddStartup = $false
    }
}

if ($AddStartup) {
    try {
        $WshShell = New-Object -ComObject WScript.Shell
        $StartupShortcut = $WshShell.CreateShortcut($Startup)
        $StartupShortcut.TargetPath = $ExePath
        $StartupShortcut.WorkingDirectory = $InstallDir
        $StartupShortcut.Description = "VietIME - Bo go Tieng Viet"
        $StartupShortcut.Save()
        Ok "Tu khoi dong: OK"
    } catch {
        Warn "Khong the tao startup shortcut: $_"
    }
} else {
    Info "Bo qua tu khoi dong. Ban co the them sau bang cach chay:"
    Write-Host "    VietIME.exe --startup" -ForegroundColor Gray
}

# 6. Hoan tat
Write-Host ""
Ok "Cai dat thanh cong!"
Write-Host ""
Write-Host "  Chay VietIME:" -ForegroundColor White
Write-Host "    VietIME" -ForegroundColor Gray
Write-Host ""
Write-Host "  Phim tat:" -ForegroundColor White
Write-Host "    Ctrl + ``     Bat/tat VietIME" -ForegroundColor Gray
Write-Host "    Ctrl + Shift  Tat Vietnamese mode" -ForegroundColor Gray
Write-Host ""
Write-Host "  Vi tri:" -ForegroundColor White
Write-Host "    $ExePath" -ForegroundColor Gray
Write-Host ""
Write-Host "  Go cai dat:" -ForegroundColor White
Write-Host "    irm https://raw.githubusercontent.com/donamvn/viet-ime/master/scripts/install.ps1 | iex -- --uninstall" -ForegroundColor Gray
Write-Host ""

# 7. Hoi mo luon
if ([Environment]::UserInteractive -and $Host.Name -eq "ConsoleHost") {
    $run = Read-Host "Mo VietIME ngay? (y/n)"
    if ($run -match "^[yY]") {
        Start-Process $ExePath
    }
}
