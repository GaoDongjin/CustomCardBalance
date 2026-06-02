$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$buildDir = Join-Path $root 'bin\Release\net9.0'
$manifest = Join-Path $root 'CustomCardBalance.json'
$dll = Join-Path $buildDir 'CustomCardBalance.dll'
$dist = Join-Path $root 'dist'
$packageRoot = Join-Path $dist 'CustomCardBalance'
$archive = Join-Path $dist 'CustomCardBalance-v2.2.0.zip'

if (-not (Test-Path -LiteralPath $dll)) {
    throw 'Missing Release DLL. Run dotnet build -c Release --no-restore first.'
}

if (Test-Path -LiteralPath $packageRoot) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}

if (Test-Path -LiteralPath $archive) {
    Remove-Item -LiteralPath $archive -Force
}

New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null
Copy-Item -LiteralPath $dll -Destination $packageRoot
Copy-Item -LiteralPath $manifest -Destination $packageRoot
Compress-Archive -LiteralPath $packageRoot -DestinationPath $archive

Write-Host "Created release archive: $archive"

