$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$dll = Join-Path $project 'bin\Release\netstandard2.1\DevSpawner.dll'
$report = Get-Content -LiteralPath (Join-Path $project 'Verification\runtime-metadata.json') -Raw | ConvertFrom-Json
[xml]$metadata = Get-Content -LiteralPath (Join-Path $project 'DevSpawner.csproj') -Raw
$version = $metadata.Project.PropertyGroup.Version
if (-not $report.passed -or $report.version -ne $version) { throw 'Matching isolated runtime checks must pass before packaging.' }
foreach ($source in $report.sources) {
    if ((Get-FileHash -LiteralPath (Join-Path $project $source.name) -Algorithm SHA256).Hash.ToLowerInvariant() -ne $source.sha256) { throw "Source changed since runtime checks: $($source.name)" }
}
$dllHash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash.ToLowerInvariant()
if ([Diagnostics.FileVersionInfo]::GetVersionInfo($dll).ProductVersion.Split('+')[0] -ne $version) { throw 'Build version does not match source.' }
$release = Join-Path $project 'Releases'
$zipPath = Join-Path $release ('KingdomInc-DevSpawner-' + $version + '.zip')
if (Test-Path -LiteralPath $zipPath) { throw 'Frozen release ZIP already exists.' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$entries = [ordered]@{
    'DevSpawner requirements.txt' = (Join-Path $project 'Nexus\REQUIREMENTS.txt')
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
    version = $version; archive = $zipPath; bytes = (Get-Item -LiteralPath $zipPath).Length
    sha256 = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
    dllSha256 = $dllHash; runtimeChecksPassed = $report.checks; cleanExtractionPassed = $true
    gameAssemblySha256 = (Get-FileHash -LiteralPath (Join-Path $project '..\..\KingdomInc_Data\Managed\Assembly-CSharp.dll')).Hash.ToLowerInvariant()
    publication = 'Prepared locally'
}
$record | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $release 'RELEASE_VERIFICATION.json') -Encoding utf8
$record | ConvertTo-Json
