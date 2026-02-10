#!/bin/bash
set -e

# ═══════════════════════════════════════════════════════
# VietIME Linux Build Script
# Build self-contained binary + AppImage
# ═══════════════════════════════════════════════════════

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
PROJECT="$ROOT_DIR/src/VietIME.Linux.App/VietIME.Linux.App.csproj"
PUBLISH_DIR="$ROOT_DIR/publish/linux"
APPDIR="$PUBLISH_DIR/VietIME.AppDir"
APP_NAME="VietIME"
ARCH="x86_64"

echo "═══════════════════════════════════════════"
echo " VietIME Linux Build"
echo "═══════════════════════════════════════════"

# 1. Clean
echo ""
echo "[1/4] Cleaning..."
rm -rf "$PUBLISH_DIR"
mkdir -p "$PUBLISH_DIR"

# 2. Publish
echo "[2/4] Publishing (linux-x64, self-contained)..."
dotnet publish "$PROJECT" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -o "$PUBLISH_DIR/bin"

echo "    → Binary: $PUBLISH_DIR/bin/VietIME"

# 3. Create AppImage structure
echo "[3/4] Creating AppImage structure..."
mkdir -p "$APPDIR/usr/bin"
mkdir -p "$APPDIR/usr/share/icons/hicolor/256x256/apps"

# Copy binary
cp "$PUBLISH_DIR/bin/VietIME" "$APPDIR/usr/bin/VietIME"
chmod +x "$APPDIR/usr/bin/VietIME"

# Copy icon (tray icon làm app icon)
if [ -f "$ROOT_DIR/src/VietIME.Linux.App/Assets/tray-icon-on.png" ]; then
    cp "$ROOT_DIR/src/VietIME.Linux.App/Assets/tray-icon-on.png" \
       "$APPDIR/usr/share/icons/hicolor/256x256/apps/vietime.png"
    cp "$ROOT_DIR/src/VietIME.Linux.App/Assets/tray-icon-on.png" \
       "$APPDIR/vietime.png"
fi

# Create .desktop file
cat > "$APPDIR/vietime.desktop" << 'EOF'
[Desktop Entry]
Type=Application
Name=VietIME
Comment=Bộ gõ Tiếng Việt cho Linux
Exec=VietIME
Icon=vietime
Categories=Utility;TextTools;
Terminal=false
StartupNotify=false
X-AppImage-Name=VietIME
X-AppImage-Version=1.0.0
EOF

# Create AppRun
cat > "$APPDIR/AppRun" << 'APPRUN'
#!/bin/bash
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
export PATH="${HERE}/usr/bin/:${PATH}"
exec "${HERE}/usr/bin/VietIME" "$@"
APPRUN
chmod +x "$APPDIR/AppRun"

# 4. Package AppImage
echo "[4/4] Packaging AppImage..."

# Kiểm tra appimagetool
if command -v appimagetool &> /dev/null; then
    appimagetool "$APPDIR" "$PUBLISH_DIR/$APP_NAME-$ARCH.AppImage"
    echo ""
    echo "✅ AppImage created: $PUBLISH_DIR/$APP_NAME-$ARCH.AppImage"
else
    echo ""
    echo "⚠️  appimagetool not found. Skipping AppImage packaging."
    echo "    Install: wget https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage"
    echo "    Or run on Linux: chmod +x appimagetool-x86_64.AppImage && sudo mv appimagetool-x86_64.AppImage /usr/local/bin/appimagetool"
    echo ""
    echo "    AppDir ready at: $APPDIR"
    echo "    Binary ready at: $PUBLISH_DIR/bin/VietIME"
fi

echo ""
echo "═══════════════════════════════════════════"
echo " Build complete!"
echo "═══════════════════════════════════════════"
echo ""
echo "Cách dùng trên Ubuntu:"
echo "  1. Copy VietIME hoặc AppImage sang Ubuntu"
echo "  2. sudo apt install xclip"
echo "  3. sudo usermod -aG input \$USER && logout"
echo "  4. Chạy: ./VietIME"
echo "  5. Hotkey: Ctrl+\` bật/tắt, Ctrl+Shift tắt Vietnamese mode"
