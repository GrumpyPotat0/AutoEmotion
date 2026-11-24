param([string]$BuildPath = "$PSScriptRoot/../AutoEmotion/bin/x64/Release")
$ErrorActionPreference = 'Stop'

foreach ($name in @('AutoEmotion.dll', 'AutoEmotion.json', 'AutoEmotion.deps.json', 'ECommons.dll', 'LICENSE', 'ECommons-LICENSE.md', 'NOTICE.md')) {
    if (!(Test-Path -LiteralPath (Join-Path $BuildPath $name))) { throw "Missing package file: $name" }
}
$manifest = Get-Content -Raw (Join-Path $BuildPath 'AutoEmotion.json') | ConvertFrom-Json
$assembly = [Reflection.AssemblyName]::GetAssemblyName((Resolve-Path (Join-Path $BuildPath 'AutoEmotion.dll'))).Version
if ($manifest.DalamudApiLevel -ne 15) { throw 'Incorrect Dalamud API level' }
if ([version]$manifest.AssemblyVersion -ne $assembly) { throw 'Manifest and assembly version differ' }
if ($manifest.InternalName -ne 'AutoEmotion') { throw 'Configuration identity changed' }
if ($manifest.Author -ne 'FoxShadow10') { throw 'Original author attribution changed' }
if ($manifest.RepoUrl -ne 'https://github.com/GrumpyPotat0/AutoEmotion') { throw 'Manifest points outside the maintenance fork' }
if (Test-Path (Join-Path $BuildPath 'Dalamud.dll')) { throw 'Do not bundle the Dalamud runtime' }
Write-Output "Package metadata and license checks passed (API 15, version $assembly)."
