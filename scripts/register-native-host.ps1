param(
    [Parameter(Mandatory = $true)]
    [string]$ManifestPath
)

$ErrorActionPreference = "Stop"

$registryKeys = @(
    "HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.directimagesaver.host",
    "HKCU:\Software\Microsoft\Edge\NativeMessagingHosts\com.directimagesaver.host"
)

foreach ($registryKey in $registryKeys) {
    New-Item -Path $registryKey -Force | Out-Null
    Set-Item -Path $registryKey -Value $ManifestPath
}

Write-Host "Registered native host manifest at $ManifestPath"
