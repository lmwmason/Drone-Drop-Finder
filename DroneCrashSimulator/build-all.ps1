$ErrorActionPreference = "Stop"
$AppProject = "src\DroneCrashSimulator.App\DroneCrashSimulator.App.csproj"
$OutRoot    = "dist"

$Targets = @(
    @{ RID = "win-x64";    Label = "Windows x64"       }
    @{ RID = "win-arm64";  Label = "Windows ARM64"      }
    @{ RID = "osx-x64";    Label = "macOS Intel"        }
    @{ RID = "osx-arm64";  Label = "macOS Apple Silicon" }
    @{ RID = "linux-x64";  Label = "Linux x64"          }
    @{ RID = "linux-arm64";Label = "Linux ARM64"         }
)

foreach ($t in $Targets) {
    $out = Join-Path $OutRoot $t.RID
    Write-Host ""
    Write-Host "==> Building $($t.Label) ($($t.RID))..." -ForegroundColor Cyan

    & "C:\Program Files\dotnet\dotnet.exe" publish $AppProject `
        -c Release `
        -r $t.RID `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -o $out `
        --nologo

    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAILED: $($t.Label)" -ForegroundColor Red
        exit 1
    }
    Write-Host "OK -> $out" -ForegroundColor Green
}

Write-Host ""
Write-Host "All builds complete." -ForegroundColor Green
Write-Host ""
Get-ChildItem $OutRoot -Directory | ForEach-Object {
    $exe = Get-ChildItem $_.FullName -File | Where-Object { $_.Extension -in ".exe","" -and $_.Name -notlike "*.pdb" } | Select-Object -First 1
    $sizeMB = if ($exe) { "{0:N1} MB" -f ($exe.Length / 1MB) } else { "?" }
    Write-Host ("  {0,-24} {1}" -f $_.Name, $sizeMB)
}
