#!/usr/bin/env bash
set -e

APP="src/DroneCrashSimulator.App/DroneCrashSimulator.App.csproj"
OUT="dist"

TARGETS=(
  "win-x64|Windows x64"
  "win-arm64|Windows ARM64"
  "osx-x64|macOS Intel"
  "osx-arm64|macOS Apple Silicon"
  "linux-x64|Linux x64"
  "linux-arm64|Linux ARM64"
)

for entry in "${TARGETS[@]}"; do
  RID="${entry%%|*}"
  LABEL="${entry##*|}"
  echo ""
  echo "==> Building $LABEL ($RID)..."
  dotnet publish "$APP" \
    -c Release \
    -r "$RID" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -o "$OUT/$RID" \
    --nologo
  echo "OK -> $OUT/$RID"
done

echo ""
echo "All builds complete."
echo ""
for d in "$OUT"/*/; do
  RID=$(basename "$d")
  SIZE=$(find "$d" -maxdepth 1 -type f \( -name "*.exe" -o -not -name "*.*" \) | head -1 | xargs du -sh 2>/dev/null | cut -f1)
  printf "  %-24s %s\n" "$RID" "$SIZE"
done
