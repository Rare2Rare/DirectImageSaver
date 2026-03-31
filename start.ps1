param(
    [switch]$SkipBuild,
    [switch]$ShowSettings
)

$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$appProject = Join-Path $repoRoot "src\DirectImageSaver.App\DirectImageSaver.App.csproj"
$appOutput = Join-Path $repoRoot "src\DirectImageSaver.App\bin\Debug\net8.0-windows"
$appExe = Join-Path $appOutput "DirectImageSaver.App.exe"

if (-not $SkipBuild -or -not (Test-Path $appExe)) {
    dotnet build $appProject | Out-Host
}

if (-not (Test-Path $appExe)) {
    throw "DirectImageSaver.App.exe was not found at $appExe"
}

$arguments = if ($ShowSettings) { "--show-settings" } else { "--background" }

Start-Process -FilePath $appExe -ArgumentList $arguments | Out-Null

Write-Host "Started DirectImageSaver from $appExe"
Write-Host "Extension folder: $repoRoot\extension"
