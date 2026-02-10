#!/bin/bash
set -euo pipefail

#═══════════════════════════════════════════════════════════════
# VietIME macOS Build Script
# Tạo .app bundle từ dotnet publish output
#
# Cách dùng:
#   ./scripts/build-mac.sh              # Build + ad-hoc sign
#   ./scripts/build-mac.sh --sign       # Build + sign với Developer ID
#═══════════════════════════════════════════════════════════════

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
PROJECT="$ROOT_DIR/src/VietIME.Mac.App/VietIME.Mac.App.csproj"
INFO_PLIST="$ROOT_DIR/src/VietIME.Mac.App/Info.plist"
ENTITLEMENTS="$ROOT_DIR/src/VietIME.Mac.App/VietIME.entitlements"

APP_NAME="VietIME"
BUNDLE_ID="com.donam.viet-ime"
OUTPUT_DIR="$ROOT_DIR/publish/mac"
APP_BUNDLE="$OUTPUT_DIR/$APP_NAME.app"

SIGN_MODE="${1:-}"

echo "╔════════════════════════════════════╗"
echo "║   VietIME macOS Build Script       ║"
echo "╚════════════════════════════════════╝"
echo ""

# ─── Step 1: Clean ───
echo "→ Cleaning previous build..."
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# ─── Step 2: Publish ───
echo "→ Publishing .NET app (self-contained, osx-arm64)..."
dotnet publish "$PROJECT" \
    -c Release \
    -r osx-arm64 \
    --self-contained true \
    -o "$OUTPUT_DIR/publish" \
    /p:PublishSingleFile=false \
    /p:PublishTrimmed=false

# ─── Step 3: Create .app bundle ───
echo "→ Creating .app bundle..."
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

# Copy executable và dependencies
cp -R "$OUTPUT_DIR/publish/"* "$APP_BUNDLE/Contents/MacOS/"

# Copy Info.plist
cp "$INFO_PLIST" "$APP_BUNDLE/Contents/"

# Copy app icon (nếu có .icns)
ICNS_FILE="$ROOT_DIR/src/VietIME.Mac.App/Assets/AppIcon.icns"
if [ -f "$ICNS_FILE" ]; then
    cp "$ICNS_FILE" "$APP_BUNDLE/Contents/Resources/"
    echo "  ✓ App icon copied"
else
    echo "  ⚠ No AppIcon.icns found (optional)"
fi

# Set executable permission
chmod +x "$APP_BUNDLE/Contents/MacOS/VietIME"

echo "  ✓ Bundle created: $APP_BUNDLE"

# ─── Step 4: Sign ───
if [ "$SIGN_MODE" = "--sign" ]; then
    echo ""
    echo "→ Signing with Developer ID..."

    # Tìm Developer ID certificate
    IDENTITY=$(security find-identity -v -p codesigning | grep "Developer ID Application" | head -1 | awk -F'"' '{print $2}')

    if [ -z "$IDENTITY" ]; then
        echo "  ✗ Không tìm thấy 'Developer ID Application' certificate!"
        echo "  → Xem hướng dẫn tạo certificate bên dưới."
        echo "  → Đang sign ad-hoc thay thế..."
        IDENTITY="-"
    else
        echo "  ✓ Found: $IDENTITY"
    fi

    # Sign tất cả .dylib trước
    echo "  → Signing libraries..."
    find "$APP_BUNDLE/Contents/MacOS" -name "*.dylib" -exec \
        codesign --force --options runtime --timestamp \
        --entitlements "$ENTITLEMENTS" \
        --sign "$IDENTITY" {} \; 2>/dev/null || true

    # Sign executable
    echo "  → Signing app bundle..."
    codesign --force --deep --options runtime --timestamp \
        --entitlements "$ENTITLEMENTS" \
        --sign "$IDENTITY" \
        "$APP_BUNDLE"

    echo "  ✓ Signed!"

    # Verify
    echo "→ Verifying signature..."
    codesign -v --verbose=2 "$APP_BUNDLE" 2>&1 | head -5
else
    echo ""
    echo "→ Ad-hoc signing (không cần certificate)..."
    codesign --force --deep \
        --entitlements "$ENTITLEMENTS" \
        --sign - \
        "$APP_BUNDLE"
    echo "  ✓ Ad-hoc signed"
fi

# ─── Step 5: Summary ───
echo ""
echo "═══════════════════════════════════════"
echo "✓ Build hoàn tất!"
echo ""
echo "  App:  $APP_BUNDLE"
echo "  Size: $(du -sh "$APP_BUNDLE" | awk '{print $1}')"
echo ""
echo "Chạy app:"
echo "  open $APP_BUNDLE"
echo ""
echo "Copy vào Applications:"
echo "  cp -R $APP_BUNDLE /Applications/"
echo "═══════════════════════════════════════"
