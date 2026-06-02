$ErrorActionPreference = 'Stop'

$settingsPanelPath = Join-Path $PSScriptRoot '..\SettingsPanel.cs'
$source = Get-Content -LiteralPath $settingsPanelPath -Raw -Encoding utf8

function Assert-SourceContains {
    param(
        [string] $Pattern,
        [string] $Message
    )

    if ($source -notmatch $Pattern) {
        throw $Message
    }
}

function Assert-SourceDoesNotContain {
    param(
        [string] $Pattern,
        [string] $Message
    )

    if ($source -match $Pattern) {
        throw $Message
    }
}

Assert-SourceContains `
    '\[HarmonyPatch\(typeof\(NGame\), nameof\(NGame\._Input\)\)\]' `
    'Missing NGame._Input Harmony patch for the F1 entry point.'

Assert-SourceContains `
    '\[HarmonyPatch\(typeof\(NSettingsScreen\), nameof\(NSettingsScreen\._Ready\)\)\]' `
    'Missing settings-screen Harmony patch for the fallback menu entry.'

Assert-SourceContains `
    'CustomCardBalanceSettings' `
    'Missing the dedicated settings row name.'

Assert-SourceDoesNotContain `
    'override void _UnhandledKeyInput' `
    'The panel still relies on _UnhandledKeyInput instead of the NGame input hook.'

Write-Host 'Hotkey entry regression checks passed.'
