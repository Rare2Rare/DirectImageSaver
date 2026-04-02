param(
    [string]$Configuration = "Release",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path $PSScriptRoot -Parent
$artifactsRoot = Join-Path $repoRoot "artifacts\publish"
$distributionRoot = $repoRoot
$installRoot = Join-Path $env:LOCALAPPDATA "DirectImageSaver\current"
$appTarget = Join-Path $installRoot "app"
$nativeHostTarget = Join-Path $installRoot "nativehost"
$extensionTarget = Join-Path $installRoot "extension"
$hostManifestPath = Join-Path $nativeHostTarget "com.directimagesaver.host.json"
$nativeHostPath = Join-Path $nativeHostTarget "DirectImageSaver.NativeHost.exe"
$trayAppPath = Join-Path $appTarget "DirectImageSaver.App.exe"
$extensionId = "kblklkfadcpplofmmfkkplglcmomicmm"

$distributionMode = (Test-Path (Join-Path $distributionRoot "app")) `
    -and (Test-Path (Join-Path $distributionRoot "nativehost")) `
    -and (Test-Path (Join-Path $distributionRoot "extension"))

if ($distributionMode) {
    $sourceRoot = $distributionRoot
}
else {
    $sourceRoot = $artifactsRoot
}

if (-not $distributionMode -and -not $SkipBuild) {
    & (Join-Path $PSScriptRoot "publish.ps1") -Configuration $Configuration
}

Remove-Item -Recurse -Force $installRoot -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $appTarget, $nativeHostTarget, $extensionTarget | Out-Null

Copy-Item (Join-Path $sourceRoot "app\*") $appTarget -Recurse -Force
Copy-Item (Join-Path $sourceRoot "nativehost\*") $nativeHostTarget -Recurse -Force
Copy-Item (Join-Path $sourceRoot "extension\*") $extensionTarget -Recurse -Force

$hostManifest = @{
    name = "com.directimagesaver.host"
    description = "DirectImageSaver Native Messaging Host"
    path = $nativeHostPath
    type = "stdio"
    allowed_origins = @(
        "chrome-extension://$extensionId/"
    )
}

$hostManifest | ConvertTo-Json -Depth 4 | Set-Content -Path $hostManifestPath -Encoding utf8

& (Join-Path $PSScriptRoot "register-native-host.ps1") -ManifestPath $hostManifestPath

$runKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
New-Item -Path $runKey -Force | Out-Null
Set-ItemProperty -Path $runKey -Name "DirectImageSaver" -Value "`"$trayAppPath`" --background"

Start-Process -FilePath $trayAppPath -ArgumentList "--show-onboarding"

Write-Host ""
Write-Host "DirectImageSaver installed."
if ($distributionMode) {
    Write-Host "Install source: distribution package root"
}
else {
    Write-Host "Install source: repository publish output"
}
Write-Host "Canonical unpacked extension path: $extensionTarget"
Write-Host "Chrome verification steps:"
Write-Host "  1. Open chrome://extensions"
Write-Host "  2. Enable Developer Mode"
Write-Host "  3. Click 'Load unpacked' and select: $extensionTarget"
Write-Host "  4. Confirm the extension name is 'DirectImageSaver'"
Write-Host "  5. Confirm the extension ID is: $extensionId"
Write-Host "  6. Click 'Reload' after reinstalling or re-registering Native Messaging"
Write-Host "  7. Refresh the target tab before testing save triggers"
Write-Host "Config file: $env:APPDATA\DirectImageSaver\config.json"
