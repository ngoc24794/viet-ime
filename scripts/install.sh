#!/bin/bash
# ═══════════════════════════════════════════════════════
# VietIME Installer — Cài đặt bằng 1 lệnh
# curl -sSL https://raw.githubusercontent.com/donamvn/viet-ime/master/scripts/install.sh | bash
# ═══════════════════════════════════════════════════════

set -e

VERSION="latest"
REPO="donamvn/viet-ime"
GITHUB="https://github.com/$REPO"
DOWNLOAD_BASE="$GITHUB/releases/latest/download"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
BOLD='\033[1m'
NC='\033[0m'

info()  { echo -e "${BLUE}[VietIME]${NC} $1"; }
ok()    { echo -e "${GREEN}[VietIME]${NC} $1"; }
warn()  { echo -e "${YELLOW}[VietIME]${NC} $1"; }
error() { echo -e "${RED}[VietIME]${NC} $1"; exit 1; }

echo ""
echo -e "${BOLD}═══════════════════════════════════════${NC}"
echo -e "${BOLD}  VietIME — Bộ gõ Tiếng Việt${NC}"
echo -e "${BOLD}═══════════════════════════════════════${NC}"
echo ""

# ─── Detect OS ───
OS="$(uname -s)"
ARCH="$(uname -m)"

case "$OS" in
    Darwin)
        PLATFORM="macOS"
        ;;
    Linux)
        PLATFORM="Linux"
        ;;
    *)
        error "Hệ điều hành không được hỗ trợ: $OS. VietIME chỉ hỗ trợ macOS và Linux."
        ;;
esac

info "Phát hiện: $PLATFORM ($ARCH)"

# ═══════════════════════════════════════
# macOS Installation
# ═══════════════════════════════════════
install_macos() {
    local DMG_URL="$DOWNLOAD_BASE/VietIME.dmg"
    local TMP_DMG="/tmp/VietIME.dmg"
    local MOUNT_POINT="/tmp/VietIME-mount"
    local APP_NAME="VietIME.app"
    local INSTALL_DIR="/Applications"

    # Kiểm tra nếu đã cài
    if [ -d "$INSTALL_DIR/$APP_NAME" ]; then
        warn "VietIME đã có trong /Applications. Sẽ cập nhật..."
        rm -rf "$INSTALL_DIR/$APP_NAME"
    fi

    # 1. Tải DMG
    info "Đang tải VietIME.dmg..."
    curl -sSL -o "$TMP_DMG" "$DMG_URL" || error "Không thể tải VietIME.dmg"
    ok "Tải xong ($(du -h "$TMP_DMG" | cut -f1 | xargs))"

    # 2. Mount DMG
    info "Đang mount DMG..."
    mkdir -p "$MOUNT_POINT"
    hdiutil attach "$TMP_DMG" -mountpoint "$MOUNT_POINT" -nobrowse -quiet || error "Không thể mount DMG"

    # 3. Copy app
    if [ -d "$MOUNT_POINT/$APP_NAME" ]; then
        info "Đang cài vào /Applications..."
        cp -R "$MOUNT_POINT/$APP_NAME" "$INSTALL_DIR/"
    else
        # Tìm .app trong DMG
        local FOUND_APP=$(find "$MOUNT_POINT" -name "*.app" -maxdepth 2 | head -1)
        if [ -n "$FOUND_APP" ]; then
            info "Đang cài $(basename "$FOUND_APP") vào /Applications..."
            cp -R "$FOUND_APP" "$INSTALL_DIR/"
            APP_NAME="$(basename "$FOUND_APP")"
        else
            hdiutil detach "$MOUNT_POINT" -quiet 2>/dev/null
            rm -f "$TMP_DMG"
            error "Không tìm thấy .app trong DMG"
        fi
    fi

    # 4. Cleanup
    hdiutil detach "$MOUNT_POINT" -quiet 2>/dev/null
    rm -f "$TMP_DMG"

    # 5. Remove quarantine
    xattr -rd com.apple.quarantine "$INSTALL_DIR/$APP_NAME" 2>/dev/null || true

    echo ""
    ok "Cài đặt thành công!"
    echo ""
    echo -e "  ${BOLD}Mở VietIME:${NC}"
    echo -e "    open /Applications/$APP_NAME"
    echo ""
    echo -e "  ${BOLD}Phím tắt:${NC}"
    echo -e "    ⌘ + \`     Bật/tắt VietIME"
    echo -e "    ⌘ + Shift  Tắt Vietnamese mode"
    echo ""
    echo -e "  ${YELLOW}Lưu ý:${NC} Lần đầu mở, vào System Settings → Privacy & Security"
    echo -e "         → Input Monitoring → cho phép VietIME."
    echo ""

    # Hỏi có muốn mở luôn không
    if [ -t 0 ]; then
        read -p "Mở VietIME ngay? (y/n) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            open "/Applications/$APP_NAME"
        fi
    fi
}

# ═══════════════════════════════════════
# Linux Installation
# ═══════════════════════════════════════
install_linux() {
    local BIN_URL="$DOWNLOAD_BASE/VietIME"
    local INSTALL_DIR="$HOME/.local/bin"
    local BIN_NAME="vietime"

    # 1. Tải binary
    info "Đang tải VietIME cho Linux (x86_64)..."
    mkdir -p "$INSTALL_DIR"
    curl -sSL -o "$INSTALL_DIR/$BIN_NAME" "$BIN_URL" || error "Không thể tải VietIME"
    chmod +x "$INSTALL_DIR/$BIN_NAME"
    ok "Tải xong ($(du -h "$INSTALL_DIR/$BIN_NAME" | cut -f1 | xargs))"

    # 2. Kiểm tra PATH
    if [[ ":$PATH:" != *":$INSTALL_DIR:"* ]]; then
        warn "$INSTALL_DIR chưa có trong PATH"

        # Thêm vào shell config
        local SHELL_RC=""
        if [ -f "$HOME/.bashrc" ]; then
            SHELL_RC="$HOME/.bashrc"
        elif [ -f "$HOME/.zshrc" ]; then
            SHELL_RC="$HOME/.zshrc"
        elif [ -f "$HOME/.profile" ]; then
            SHELL_RC="$HOME/.profile"
        fi

        if [ -n "$SHELL_RC" ]; then
            if ! grep -q "$INSTALL_DIR" "$SHELL_RC" 2>/dev/null; then
                echo "" >> "$SHELL_RC"
                echo "# VietIME" >> "$SHELL_RC"
                echo "export PATH=\"\$HOME/.local/bin:\$PATH\"" >> "$SHELL_RC"
                ok "Đã thêm $INSTALL_DIR vào PATH trong $(basename "$SHELL_RC")"
            fi
        fi

        export PATH="$INSTALL_DIR:$PATH"
    fi

    # 3. Cài xclip (cần cho clipboard paste)
    info "Kiểm tra dependencies..."
    if ! command -v xclip &> /dev/null; then
        warn "xclip chưa được cài (cần cho gõ tiếng Việt)"
        if command -v apt &> /dev/null; then
            info "Đang cài xclip..."
            sudo apt install -y xclip 2>/dev/null && ok "Đã cài xclip" || warn "Không thể tự cài xclip. Chạy: sudo apt install xclip"
        elif command -v dnf &> /dev/null; then
            info "Đang cài xclip..."
            sudo dnf install -y xclip 2>/dev/null && ok "Đã cài xclip" || warn "Không thể tự cài xclip. Chạy: sudo dnf install xclip"
        elif command -v pacman &> /dev/null; then
            info "Đang cài xclip..."
            sudo pacman -S --noconfirm xclip 2>/dev/null && ok "Đã cài xclip" || warn "Không thể tự cài xclip. Chạy: sudo pacman -S xclip"
        else
            warn "Cần cài xclip thủ công cho distro của bạn"
        fi
    else
        ok "xclip đã có"
    fi

    # 4. Kiểm tra quyền input
    if [ -e /dev/input/event0 ]; then
        if ! [ -r /dev/input/event0 ]; then
            warn "Chưa có quyền đọc /dev/input"
            if command -v usermod &> /dev/null; then
                info "Thêm user vào group 'input'..."
                sudo usermod -aG input "$USER" 2>/dev/null && ok "Đã thêm $USER vào group input" || warn "Không thể tự thêm. Chạy: sudo usermod -aG input \$USER"
                warn "Cần logout rồi login lại để có hiệu lực!"
            fi
        else
            ok "Quyền /dev/input OK"
        fi
    fi

    # 5. Tải và cài đặt icon
    info "Cài đặt icon..."
    local ICON_DIR="$HOME/.local/share/icons/hicolor/scalable/apps"
    local ICON_256_DIR="$HOME/.local/share/icons/hicolor/256x256/apps"
    local ICON_URL="https://raw.githubusercontent.com/$REPO/master/src/VietIME.Linux.App/Assets/vietime.svg"
    local ICON_PNG_URL="https://raw.githubusercontent.com/$REPO/master/src/VietIME.Linux.App/Assets/tray-icon-on.png"

    mkdir -p "$ICON_DIR" "$ICON_256_DIR"
    curl -sSL -o "$ICON_DIR/vietime.svg" "$ICON_URL" 2>/dev/null && ok "Icon SVG đã cài" || true
    curl -sSL -o "$ICON_256_DIR/vietime.png" "$ICON_PNG_URL" 2>/dev/null || true

    # Cập nhật icon cache
    gtk-update-icon-cache -f "$HOME/.local/share/icons/hicolor" 2>/dev/null || true

    # 6. Tạo .desktop file (cho app launcher)
    local APP_DESKTOP_DIR="$HOME/.local/share/applications"
    mkdir -p "$APP_DESKTOP_DIR"
    cat > "$APP_DESKTOP_DIR/vietime.desktop" << EOF
[Desktop Entry]
Type=Application
Name=VietIME
GenericName=Vietnamese Input Method
Comment=Bộ gõ Tiếng Việt cho Linux
Exec=$INSTALL_DIR/$BIN_NAME
Icon=vietime
Categories=Utility;TextTools;System;
Terminal=false
StartupNotify=false
Keywords=Vietnamese;Input;IME;Telex;VNI;
EOF

    # Cập nhật desktop database
    update-desktop-database "$APP_DESKTOP_DIR" 2>/dev/null || true

    # 7. Tạo shortcut trên Desktop
    local USER_DESKTOP="$HOME/Desktop"
    if [ ! -d "$USER_DESKTOP" ]; then
        # Thử xdg-user-dir (hỗ trợ tên thư mục tiếng Việt/locale khác)
        USER_DESKTOP=$(xdg-user-dir DESKTOP 2>/dev/null || echo "$HOME/Desktop")
    fi

    if [ -d "$USER_DESKTOP" ]; then
        cp "$APP_DESKTOP_DIR/vietime.desktop" "$USER_DESKTOP/vietime.desktop"
        chmod +x "$USER_DESKTOP/vietime.desktop"
        # Đánh dấu trusted (GNOME 42+)
        gio set "$USER_DESKTOP/vietime.desktop" metadata::trusted true 2>/dev/null || true
        ok "Đã tạo shortcut trên Desktop"
    fi

    echo ""
    ok "Cài đặt thành công!"
    echo ""
    echo -e "  ${BOLD}Chạy VietIME:${NC}"
    echo -e "    vietime"
    echo ""
    echo -e "  ${BOLD}Phím tắt:${NC}"
    echo -e "    Ctrl + \`     Bật/tắt VietIME"
    echo -e "    Ctrl + Shift  Tắt Vietnamese mode"
    echo ""

    # Kiểm tra cần logout
    if [ -e /dev/input/event0 ] && ! [ -r /dev/input/event0 ]; then
        echo -e "  ${YELLOW}⚠ Quan trọng:${NC} Logout rồi login lại để quyền input có hiệu lực!"
        echo ""
    fi

    # 6. Tạo systemd user service (tự khởi động)
    local SERVICE_DIR="$HOME/.config/systemd/user"
    if [ -d "$(dirname "$SERVICE_DIR")" ] || command -v systemctl &> /dev/null; then
        mkdir -p "$SERVICE_DIR"
        cat > "$SERVICE_DIR/vietime.service" << EOF
[Unit]
Description=VietIME - Bộ gõ Tiếng Việt
After=graphical-session.target

[Service]
Type=simple
ExecStart=$INSTALL_DIR/$BIN_NAME
Restart=on-failure
RestartSec=3

[Install]
WantedBy=default.target
EOF
        info "Đã tạo systemd service. Để tự khởi động cùng hệ thống:"
        echo -e "    systemctl --user enable vietime"
        echo -e "    systemctl --user start vietime"
        echo ""
    fi
}

# ═══════════════════════════════════════
# Gỡ cài đặt
# ═══════════════════════════════════════
uninstall() {
    info "Gỡ cài đặt VietIME..."

    case "$PLATFORM" in
        macOS)
            rm -rf "/Applications/VietIME.app"
            ok "Đã xóa /Applications/VietIME.app"
            ;;
        Linux)
            rm -f "$HOME/.local/bin/vietime"
            rm -f "$HOME/.local/share/applications/vietime.desktop"
            rm -f "$(xdg-user-dir DESKTOP 2>/dev/null || echo "$HOME/Desktop")/vietime.desktop"
            rm -f "$HOME/.local/share/icons/hicolor/scalable/apps/vietime.svg"
            rm -f "$HOME/.local/share/icons/hicolor/256x256/apps/vietime.png"
            rm -f "$HOME/.config/systemd/user/vietime.service"
            systemctl --user disable vietime 2>/dev/null || true
            ok "Đã gỡ VietIME"
            ;;
    esac

    echo ""
}

# ═══════════════════════════════════════
# Main
# ═══════════════════════════════════════
if [ "${1:-}" = "--uninstall" ] || [ "${1:-}" = "uninstall" ]; then
    uninstall
    exit 0
fi

case "$PLATFORM" in
    macOS)  install_macos ;;
    Linux)  install_linux ;;
esac
