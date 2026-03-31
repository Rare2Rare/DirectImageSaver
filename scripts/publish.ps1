param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path $PSScriptRoot -Parent
$artifactsRoot = Join-Path $repoRoot "artifacts\publish"
$appOutput = Join-Path $artifactsRoot "app"
$nativeHostOutput = Join-Path $artifactsRoot "nativehost"
$extensionOutput = Join-Path $artifactsRoot "extension"

Remove-Item -Recurse -Force $artifactsRoot -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $appOutput, $nativeHostOutput, $extensionOutput | Out-Null

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

Write-Host "Published application to $artifactsRoot"
