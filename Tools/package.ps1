$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$dll = Join-Path $project 'bin\Release\netstandard2.1\DevSpawner.dll'
$report = Get-Content -LiteralPath (Join-Path $project 'Verification\runtime-tests.txt')
if ($report[0] -ne 'passed=True' -or @($report | Where-Object { $_.Trim() }).Count -ne 19) { throw 'All 18 runtime checks must pass before packaging.' }
$dllHash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash.ToLowerInvariant()
if ($dllHash -ne '0606f58238a5ba0737becb2f427204abb1dbc3cd7a5491bd6db0ecdf7a8fe47d') { throw 'Normal build differs from the verified 1.1.0 build.' }
$release = Join-Path $project 'Releases'
$zipPath = Join-Path $release 'KingdomInc-DevSpawner-1.1.0.zip'
if (Test-Path -LiteralPath $zipPath) { throw 'Frozen release ZIP already exists.' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$entries = [ordered]@{
    'BepInEx/plugins/DevSpawner/DevSpawner.dll' = $dll
    'DevSpawner README.txt' = (Join-Path $project 'Nexus\README.txt')
}
$archive = [IO.Compression.ZipFile]::Open($zipPath, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($entry in $entries.GetEnumerator()) { [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$entry.Value,$entry.Key) | Out-Null }
} finally { $archive.Dispose() }
$extracted = Join-Path $project ('Verification\Package-' + [Guid]::NewGuid().ToString('N'))
[IO.Compression.ZipFile]::ExtractToDirectory($zipPath, $extracted)
if (@(Get-ChildItem -LiteralPath $extracted -Recurse -File).Count -ne $entries.Count) { throw 'Unexpected archive entries.' }
foreach ($entry in $entries.GetEnumerator()) {
    if ((Get-FileHash -LiteralPath (Join-Path $extracted $entry.Key)).Hash -ne (Get-FileHash -LiteralPath $entry.Value).Hash) { throw "Clean extraction differs: $($entry.Key)" }
}
$record = [ordered]@{
    version = '1.1.0'; archive = $zipPath; bytes = (Get-Item -LiteralPath $zipPath).Length
    sha256 = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
    dllSha256 = $dllHash; runtimeChecksPassed = 18; cleanExtractionPassed = $true
    gameAssemblySha256 = (Get-FileHash -LiteralPath (Join-Path $project '..\..\KingdomInc_Data\Managed\Assembly-CSharp.dll')).Hash.ToLowerInvariant()
    publication = 'Prepared for authorized Nexus release'
}
$record | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $release 'RELEASE_VERIFICATION.json') -Encoding utf8
$record | ConvertTo-Json
