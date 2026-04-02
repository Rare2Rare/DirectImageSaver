param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path $PSScriptRoot -Parent
$artifactsRoot = Join-Path $repoRoot "artifacts\publish"
$appOutput = Join-Path $artifactsRoot "app"
$nativeHostOutput = Join-Path $artifactsRoot "nativehost"
$extensionOutput = Join-Path $artifactsRoot "extension"
$scriptsOutput = Join-Path $artifactsRoot "scripts"
$zipPath = Join-Path $repoRoot "artifacts\DirectImageSaver.zip"

Remove-Item -Recurse -Force $artifactsRoot -ErrorAction SilentlyContinue
Remove-Item -Force $zipPath -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $appOutput, $nativeHostOutput, $extensionOutput, $scriptsOutput | Out-Null

dotnet publish (Join-Path $repoRoot "src\DirectImageSaver.App\DirectImageSaver.App.csproj") `
    -c $Configuration `
    -r win-x64 `
    --self-contained false `
    -o $appOutput

dotnet publish (Join-Path $repoRoot "src\DirectImageSaver.NativeHost\DirectImageSaver.NativeHost.csproj") `
    -c $Configuration `
    -r win-x64 `
    --self-contained false `
    -o $nativeHostOutput

Copy-Item (Join-Path $repoRoot "extension\*") $extensionOutput -Recurse -Force
Copy-Item (Join-Path $repoRoot "scripts\install.ps1") $scriptsOutput -Force
Copy-Item (Join-Path $repoRoot "scripts\uninstall.ps1") $scriptsOutput -Force
Copy-Item (Join-Path $repoRoot "scripts\register-native-host.ps1") $scriptsOutput -Force
Copy-Item (Join-Path $repoRoot "README.md") $artifactsRoot -Force

Compress-Archive -Path (Join-Path $artifactsRoot '*') -DestinationPath $zipPath -Force

Write-Host "Published application to $artifactsRoot"
Write-Host "Created distributable ZIP at $zipPath"
