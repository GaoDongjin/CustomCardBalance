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

Assert-SourceContains `
    'private const float EffectWidth = 450f;' `
    'The effect columns are not widened to the approved layout width.'

Assert-SourceContains `
    'private const float EntryRowHeight = 72f;' `
    'The card rows are not tall enough for wrapped effect text.'

Assert-SourceContains `
    'Cell\(entry\.BaseEffect, EffectWidth, StsColors\.cream, 16, EntryRowHeight, true\)' `
    'The base-effect cell is not configured as a wrapped effect cell.'

Assert-SourceContains `
    'Cell\(entry\.UpgradedEffect, EffectWidth, StsColors\.cream, 16, EntryRowHeight, true\)' `
    'The upgraded-effect cell is not configured as a wrapped effect cell.'

Assert-SourceContains `
    'label\.ClipText = !wrap;' `
    'Wrapped effect cells are still clipped.'

Assert-SourceContains `
    'row\.AddChild\(Spacer\(\)\);[\s\S]*?row\.AddChild\(toggle\);' `
    'The switch column is not pushed to the right edge with flexible space.'

Assert-SourceContains `
    'PaddedCell\(entry\.Name, CardNameWidth, CategoryColor\(entry\.Category\), 20, EntryRowHeight, CardNameLeftPadding\)' `
    'The card-name text is not shifted slightly to the right.'

Write-Host 'Settings layout regression checks passed.'
