param(
    [string]$ChromiumManifestPath,
    [string]$FirefoxManifestPath,
    [string]$ManifestPath  # Backward-compat alias for $ChromiumManifestPath
)

$ErrorActionPreference = "Stop"

if (-not $ChromiumManifestPath -and $ManifestPath) {
    $ChromiumManifestPath = $ManifestPath
}

if ($ChromiumManifestPath) {
    $chromiumKeys = @(
        "HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.directimagesaver.host",
        "HKCU:\Software\Microsoft\Edge\NativeMessagingHosts\com.directimagesaver.host"
    )
    foreach ($key in $chromiumKeys) {
        New-Item -Path $key -Force | Out-Null
        Set-Item -Path $key -Value $ChromiumManifestPath
    }
    Write-Host "Registered Chrome/Edge native host manifest at $ChromiumManifestPath"
}

if ($FirefoxManifestPath) {
    $firefoxKey = "HKCU:\Software\Mozilla\NativeMessagingHosts\com.directimagesaver.host"
    New-Item -Path $firefoxKey -Force | Out-Null
    Set-Item -Path $firefoxKey -Value $FirefoxManifestPath
    Write-Host "Registered Firefox native host manifest at $FirefoxManifestPath"
}

if (-not $ChromiumManifestPath -and -not $FirefoxManifestPath) {
    throw "At least one of -ChromiumManifestPath or -FirefoxManifestPath must be specified."
}
