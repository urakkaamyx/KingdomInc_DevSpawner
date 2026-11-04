param([switch]$WaitForExit)
$ErrorActionPreference = 'Stop'
$gameRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$releaseFolder = Join-Path $PSScriptRoot 'Releases'
$source = Join-Path $releaseFolder 'DevSpawner-1.1.0.dll'
$destination = Join-Path $gameRoot 'BepInEx\plugins\DevSpawner\DevSpawner.dll'
$statusPath = Join-Path $releaseFolder 'installation-status.json'
$expectedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $source).Hash
function Write-Status([string]$state, [string]$detail) {
    @{ state = $state; detail = $detail; version = '1.1.0'; sha256 = $expectedHash; updatedAtUtc = [DateTime]::UtcNow.ToString('o') } |
        ConvertTo-Json | Set-Content -LiteralPath $statusPath -Encoding UTF8
}
function Get-RunningGame {
    Get-CimInstance Win32_Process -Filter "Name='KingdomInc.exe'" |
        Where-Object { $_.ExecutablePath -eq (Join-Path $gameRoot 'KingdomInc.exe') }
}
try {
    Write-Status 'waiting' 'Waiting for Kingdom Inc to close before installing.'
    while (Get-RunningGame) {
        if (-not $WaitForExit) { throw 'Close Kingdom Inc before installing, or use -WaitForExit.' }
        Start-Sleep -Seconds 2
    }
    if (Test-Path -LiteralPath $destination) {
        $oldVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($destination).FileVersion
        $backup = Join-Path $releaseFolder ('DevSpawner-' + $oldVersion + '-previous.dll')
        if (-not (Test-Path -LiteralPath $backup)) { Copy-Item -LiteralPath $destination -Destination $backup }
    }
    Copy-Item -LiteralPath $source -Destination $destination -Force
    if ((Get-FileHash -Algorithm SHA256 -LiteralPath $destination).Hash -ne $expectedHash) { throw 'Installed DLL checksum does not match staged update.' }
    Write-Status 'installed' 'DevSpawner 1.1.0 installed and checksum verified. Start Kingdom Inc to use it.'
} catch {
    Write-Status 'failed' $_.Exception.Message
    throw
}
