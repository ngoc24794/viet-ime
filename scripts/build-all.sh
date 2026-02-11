#!/bin/bash

# ═══════════════════════════════════════════════════════
# VietIME — Build tất cả platforms từ Mac
# Chạy: bash scripts/build-all.sh [--release]
# ═══════════════════════════════════════════════════════

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
CONFIG="${1:---debug}"

if [ "$CONFIG" = "--release" ]; then
    BUILD_CONFIG="Release"
    echo "═══════════════════════════════════════════"
    echo " VietIME — Build ALL (Release)"
    echo "═══════════════════════════════════════════"
else
    BUILD_CONFIG="Debug"
    echo "═══════════════════════════════════════════"
    echo " VietIME — Build ALL (Debug)"
    echo "═══════════════════════════════════════════"
fi

ERRORS=0

# ─── 1. Shared Core ───
echo ""
echo "▸ [1/4] VietIME.Core (shared)"
if dotnet build "$ROOT_DIR/src/VietIME.Core" -c "$BUILD_CONFIG" --nologo -q; then
    echo "  ✓ Core OK"
else
    echo "  ✗ Core FAILED"
    ERRORS=$((ERRORS + 1))
fi

# ─── 2. macOS ───
echo ""
echo "▸ [2/4] macOS (VietIME.Mac.App)"
if dotnet build "$ROOT_DIR/src/VietIME.Mac.App" -c "$BUILD_CONFIG" --nologo -q; then
    echo "  ✓ macOS OK"
else
    echo "  ✗ macOS FAILED"
    ERRORS=$((ERRORS + 1))
fi

# ─── 3. Linux (cross-compile) ───
echo ""
echo "▸ [3/4] Linux (VietIME.Linux.App — cross-compile)"
if dotnet build "$ROOT_DIR/src/VietIME.Linux.App" -c "$BUILD_CONFIG" --nologo -q; then
    echo "  ✓ Linux OK"
else
    echo "  ✗ Linux FAILED"
    ERRORS=$((ERRORS + 1))
fi

# ─── 4. Windows (chỉ Hook + Core, WPF không build được trên Mac) ───
echo ""
echo "▸ [4/4] Windows (VietIME.Hook — cross-compile)"
if dotnet build "$ROOT_DIR/src/VietIME.Hook" -c "$BUILD_CONFIG" --nologo -q; then
    echo "  ✓ Windows Hook OK"
    echo "  ⚠ VietIME.App (WPF) cần build trên Windows"
else
    echo "  ✗ Windows Hook FAILED"
    ERRORS=$((ERRORS + 1))
fi

# ─── Kết quả ───
echo ""
echo "═══════════════════════════════════════════"
if [ $ERRORS -eq 0 ]; then
    echo " ✅ Build thành công! ($BUILD_CONFIG)"
    echo ""
    echo " Từ Mac có thể build:"
    echo "   ✓ VietIME.Core      (shared — ảnh hưởng cả 3 nền tảng)"
    echo "   ✓ VietIME.Mac.App   (macOS native)"
    echo "   ✓ VietIME.Linux.App (Linux cross-compile)"
    echo "   ✓ VietIME.Hook      (Windows hook library)"
    echo "   ✗ VietIME.App       (WPF — cần Windows để build)"
else
    echo " ❌ Build có $ERRORS lỗi!"
fi
echo "═══════════════════════════════════════════"

# ─── Publish (nếu --release) ───
if [ "$BUILD_CONFIG" = "Release" ]; then
    echo ""
    echo "▸ Publishing binaries..."

    # macOS
    echo "  → macOS (osx-arm64)..."
    dotnet publish "$ROOT_DIR/src/VietIME.Mac.App" -c Release -r osx-arm64 \
        --self-contained true -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -o "$ROOT_DIR/publish/mac" --nologo -q
    echo "  ✓ publish/mac/"

    # Linux
    echo "  → Linux (linux-x64)..."
    dotnet publish "$ROOT_DIR/src/VietIME.Linux.App" -c Release -r linux-x64 \
        --self-contained true -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:EnableCompressionInSingleFile=true \
        -o "$ROOT_DIR/publish/linux" --nologo -q
    echo "  ✓ publish/linux/"

    echo ""
    echo " 📦 Binaries:"
    ls -lh "$ROOT_DIR/publish/mac/VietIME" 2>/dev/null || true
    ls -lh "$ROOT_DIR/publish/linux/VietIME" 2>/dev/null || true
    echo ""
    echo " ⚠ Windows .exe cần build trên Windows:"
    echo "   dotnet publish src/VietIME.App -c Release -o publish/win"
fi

exit $ERRORS
