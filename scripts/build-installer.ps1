param(
    [string]$Configuration = "Release",
    [string]$AppVersion = "0.1.0",
    [switch]$SkipPublish
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
$publishRoot = Join-Path $repoRoot "artifacts\publish"
$outputRoot = Join-Path $repoRoot "artifacts"
$installerScript = Join-Path $repoRoot "installer\DirectImageSaver.iss"

if (-not $SkipPublish) {
    & (Join-Path $PSScriptRoot "publish.ps1") -Configuration $Configuration -SkipInstaller
}

if (-not (Test-Path $publishRoot)) {
    throw "Publish output was not found: $publishRoot"
}

if (-not (Test-Path $installerScript)) {
    throw "Inno Setup script was not found: $installerScript"
}

$isccPath = $null
$iscc = Get-Command ISCC -ErrorAction SilentlyContinue
if ($null -ne $iscc) {
    $isccPath = $iscc.Source
}

if ($null -eq $isccPath) {
    $candidatePaths = @(
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe",
        (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe")
    )

    $isccPath = $candidatePaths | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if ($null -eq $isccPath) {
    $uninstallEntries = @(
        "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*",
        "HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*",
        "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*"
    )

    $installLocation = Get-ItemProperty $uninstallEntries -ErrorAction SilentlyContinue |
        Where-Object { $_.DisplayName -like "Inno Setup*" } |
        Select-Object -ExpandProperty InstallLocation -First 1

    if (-not [string]::IsNullOrWhiteSpace($installLocation)) {
        $candidate = Join-Path $installLocation "ISCC.exe"
        if (Test-Path $candidate) {
            $isccPath = $candidate
        }
    }
}

if ($null -eq $isccPath) {
    throw "ISCC.exe was not found. Install Inno Setup 6 and run scripts\\build-installer.ps1 again."
}

Invoke-NativeCommand -ErrorMessage "Failed to build DirectImageSaver installer." -Command {
    & $isccPath `
        "/DPayloadDir=$publishRoot" `
        "/DOutputDir=$outputRoot" `
        "/DAppVersion=$AppVersion" `
        $installerScript
}

Write-Host "Created installer at $(Join-Path $outputRoot 'DirectImageSaver-Setup.exe')"
