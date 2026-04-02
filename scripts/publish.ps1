param(
    [string]$Configuration = "Release",
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Command,
        [Parameter(Mandatory = $true)]
        [string]$ErrorMessage
    )

    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw $ErrorMessage
    }
}

$repoRoot = Split-Path $PSScriptRoot -Parent
$artifactsRoot = Join-Path $repoRoot "artifacts"
$publishRoot = Join-Path $artifactsRoot "publish"
$appOutput = Join-Path $publishRoot "app"
$nativeHostOutput = Join-Path $publishRoot "nativehost"
$extensionOutput = Join-Path $publishRoot "extension"
$scriptsOutput = Join-Path $publishRoot "scripts"
$zipPath = Join-Path $artifactsRoot "DirectImageSaver.zip"
$quickStartPath = Join-Path $repoRoot "QUICKSTART.ja.md"

Remove-Item -Recurse -Force $publishRoot -ErrorAction SilentlyContinue
Remove-Item -Force $zipPath -ErrorAction SilentlyContinue
Remove-Item -Force (Join-Path $artifactsRoot "DirectImageSaver-Setup.exe") -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $appOutput, $nativeHostOutput, $extensionOutput, $scriptsOutput | Out-Null

Invoke-NativeCommand -ErrorMessage "Failed to publish DirectImageSaver.App." -Command {
    dotnet publish (Join-Path $repoRoot "src\DirectImageSaver.App\DirectImageSaver.App.csproj") `
        -c $Configuration `
        -r win-x64 `
        --self-contained true `
        -o $appOutput
}

Invoke-NativeCommand -ErrorMessage "Failed to publish DirectImageSaver.NativeHost." -Command {
    dotnet publish (Join-Path $repoRoot "src\DirectImageSaver.NativeHost\DirectImageSaver.NativeHost.csproj") `
        -c $Configuration `
        -r win-x64 `
        --self-contained true `
        -o $nativeHostOutput
}

Copy-Item (Join-Path $repoRoot "extension\*") $extensionOutput -Recurse -Force
Copy-Item (Join-Path $repoRoot "scripts\install.ps1") $scriptsOutput -Force
Copy-Item (Join-Path $repoRoot "scripts\uninstall.ps1") $scriptsOutput -Force
Copy-Item (Join-Path $repoRoot "scripts\register-native-host.ps1") $scriptsOutput -Force
Copy-Item (Join-Path $repoRoot "README.md") $publishRoot -Force

if (Test-Path $quickStartPath) {
    Copy-Item $quickStartPath $publishRoot -Force
}

Compress-Archive -Path (Join-Path $publishRoot '*') -DestinationPath $zipPath -Force

if (-not $SkipInstaller) {
    & (Join-Path $PSScriptRoot "build-installer.ps1") -Configuration $Configuration -SkipPublish
}

Write-Host "Published application to $publishRoot"
Write-Host "Created distributable ZIP at $zipPath"
