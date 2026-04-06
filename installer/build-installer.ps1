$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $scriptDir "..\EnvVar.csproj"
$outputDir = Join-Path $scriptDir "..\release\publish"
$issPath = Join-Path $scriptDir "EnvVar.iss"

Write-Host "==> Publishing application..."

dotnet publish $projectPath `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true `
  -o $outputDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed"
    exit 1
}

Write-Host "==> Building installer..."

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

& $iscc $issPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Installer build failed"
    exit 1
}

Write-Host "==> Done! Installer located in /release"