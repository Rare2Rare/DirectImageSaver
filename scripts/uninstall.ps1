param(
    [switch]$RemoveUserData
)

$ErrorActionPreference = "Stop"

$installRoot = Join-Path $env:LOCALAPPDATA "DirectImageSaver\current"
$runKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
$registryKeys = @(
    "HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.directimagesaver.host",
    "HKCU:\Software\Microsoft\Edge\NativeMessagingHosts\com.directimagesaver.host",
    "HKCU:\Software\Mozilla\NativeMessagingHosts\com.directimagesaver.host"
)

foreach ($registryKey in $registryKeys) {
    Remove-Item -Path $registryKey -Recurse -Force -ErrorAction SilentlyContinue
}

Remove-ItemProperty -Path $runKey -Name "DirectImageSaver" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force $installRoot -ErrorAction SilentlyContinue

if ($RemoveUserData) {
    Remove-Item -Recurse -Force (Join-Path $env:APPDATA "DirectImageSaver") -ErrorAction SilentlyContinue
}

Write-Host "DirectImageSaver uninstall completed."
