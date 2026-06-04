$ErrorActionPreference = 'Stop'

$root = Join-Path $PSScriptRoot '..'
$plugin = Get-Content -LiteralPath (Join-Path $root 'Plugin.cs') -Raw -Encoding utf8
$installer = Get-Content -LiteralPath (Join-Path $root 'HarmonyPatchInstaller.cs') -Raw -Encoding utf8

function Assert-Contains {
    param(
        [string] $Source,
        [string] $Pattern,
        [string] $Message
    )

    if ($Source -notmatch $Pattern) {
        throw $Message
    }
}

function Assert-DoesNotContain {
    param(
        [string] $Source,
        [string] $Pattern,
        [string] $Message
    )

    if ($Source -match $Pattern) {
        throw $Message
    }
}

Assert-DoesNotContain $plugin 'PatchAll\(' 'Startup still uses Harmony PatchAll and scans the whole assembly.'
Assert-Contains $plugin 'HarmonyPatchInstaller\.Install\(harmony\)' 'Plugin.Initialize does not use the explicit patch installer.'
Assert-Contains $installer 'public static class HarmonyPatchInstaller' 'The explicit Harmony patch installer is missing.'
Assert-Contains $installer 'PatchPostfix\(harmony, typeof\(NMainMenu\), nameof\(NMainMenu\._Ready\)' 'The main-menu settings panel patch is not explicitly installed.'
Assert-Contains $installer 'PatchPrefix\(harmony, typeof\(NGame\), nameof\(NGame\._Input\)' 'The F1 hotkey patch is not explicitly installed.'
Assert-Contains $installer 'PatchPostfix\(harmony, typeof\(NSettingsScreen\), nameof\(NSettingsScreen\._Ready\)' 'The settings-menu fallback patch is not explicitly installed.'
Assert-Contains $installer 'PatchPrefix\(harmony, typeof\(LocTable\), nameof\(LocTable\.GetRawText\)' 'The localization description patch is not explicitly installed.'

Write-Host 'Startup patching regression checks passed.'
