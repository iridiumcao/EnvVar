param (
    [string]$version = "1.0.0"
)

$scriptDir = $PSScriptRoot
$projectPath = Join-Path $scriptDir "..\EnvVar.csproj"
$outputDir = Join-Path $scriptDir "..\release\publish"
$issPath = Join-Path $scriptDir "EnvVar.iss"

# Clean up previous build artifacts
if (Test-Path $outputDir) {
    Remove-Item -Recurse -Force $outputDir
}

Write-Host "==> Publishing application (Version: $version)..."

dotnet publish $projectPath `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:Version=$version `
  -o $outputDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed"
    exit 1
}

Write-Host "==> Building installer (Version: $version)..."

$iscc = Get-Command iscc -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source

if (!$iscc) {
    $defaultPath = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
    if (Test-Path $defaultPath) {
        $iscc = $defaultPath
    }
}

if (!$iscc) {
    Write-Error "Inno Setup (iscc.exe) not found in PATH or default directory. Please install it first or add it to your PATH."
    exit 1
}

& $iscc $issPath /dAppVersion=$version /dAppVersionName=$version

if ($LASTEXITCODE -ne 0) {
    Write-Error "Installer build failed"
    exit 1
}

Write-Host "==> Done! Installer located in /release"
