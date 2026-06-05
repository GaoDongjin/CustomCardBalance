$ErrorActionPreference = 'Stop'

$root = Join-Path $PSScriptRoot '..'
$configuration = Get-Content -LiteralPath (Join-Path $root 'ModConfiguration.cs') -Raw -Encoding utf8
$settingsPanel = Get-Content -LiteralPath (Join-Path $root 'SettingsPanel.cs') -Raw -Encoding utf8
$patches = Get-Content -LiteralPath (Join-Path $root 'CardPatches.cs') -Raw -Encoding utf8
$manifest = Get-Content -LiteralPath (Join-Path $root 'CustomCardBalance.json') -Raw -Encoding utf8

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

Assert-Contains $manifest '"id": "CustomCardBalance"' 'The internal manifest ID is not CustomCardBalance.'
Assert-Contains $manifest '"name": "Custom Card Balance"' 'The public mod name is not Custom Card Balance.'
Assert-Contains $manifest '"author": "Bruiser"' 'The manifest author is not Bruiser.'
Assert-Contains $manifest '"version": "2\.2\.0"' 'The manifest version is not 2.2.0.'
Assert-Contains $manifest '21 ' 'The manifest description does not mention 21 configurable cards.'
Assert-Contains $configuration 'Path\.Combine\(OS\.GetUserDataDir\(\), "CustomCardBalance", "settings\.json"\)' 'The active CustomCardBalance settings path is missing.'

$cardIds = @(
    'Dominate',
    'ExpectAFight',
    'ForgottenRitual',
    'Acrobatics',
    'Untouchable',
    'Anticipate',
    'Speedster',
    'Murder',
    'WraithForm',
    'Voltaic',
    'Rainbow',
    'Glow',
    'Alignment',
    'VoidForm',
    'TheSealedThrone',
    'BansheesCry',
    'Dirge',
    'Seance',
    'BorrowedTime',
    'Defy',
    'Production'
)

foreach ($cardId in $cardIds) {
    Assert-Contains $configuration "public const string $cardId = " "Missing card ID: $cardId"
    Assert-Contains $settingsPanel "CardIds\.$cardId" "Missing settings-panel row: $cardId"
}

$entryCount = ([regex]::Matches($settingsPanel, 'new CardBalanceEntry\(')).Count
if ($entryCount -ne 21) {
    throw "Expected 21 menu rows, found $entryCount."
}

Assert-Contains $settingsPanel 'Custom Card Balance ' 'The panel title was not renamed.'
Assert-Contains $settingsPanel 'row\.GetNode<RichTextLabel>\("Label"\)\.Text = "Custom Card Balance";' 'The settings-menu entry was not renamed.'

$requiredPatchClasses = @(
    'DominateUpgradePatch',
    'ExpectAFightPlayPatch',
    'ForgottenRitualKeywordsPatch',
    'ForgottenRitualHoverTipsPatch',
    'MurderUpgradePatch',
    'AlignmentStarCostPatch',
    'GlowVarsPatch',
    'GlowPlayPatch',
    'VoidFormKeywordsPatch',
    'VoidFormUpgradePatch',
    'TheSealedThroneUpgradePatch',
    'BorrowedTimeVarsPatch',
    'BorrowedTimeHoverTipsPatch',
    'BorrowedTimePlayPatch',
    'BorrowedTimeUpgradePatch',
    'ProductionUpgradePatch'
)

foreach ($patchClass in $requiredPatchClasses) {
    Assert-Contains $patches "class $patchClass" "Missing patch class: $patchClass"
}

Assert-Contains $patches 'CardIds\.Voltaic' 'Missing Voltaic cost adjustment.'
Assert-Contains $patches 'CardIds\.Rainbow' 'Missing Rainbow cost adjustment.'
Assert-Contains $patches 'CardIds\.BorrowedTime' 'Missing BorrowedTime adjustment.'
Assert-Contains $patches '"EXPECT_A_FIGHT\.description"' 'Missing Expect A Fight description adjustment.'
Assert-Contains $patches '"GLOW\.description"' 'Missing Glow description adjustment.'
Assert-Contains $patches '"BORROWED_TIME\.description"' 'Missing Borrowed Time description adjustment.'

Write-Host 'Custom Card Balance expansion regression checks passed.'
