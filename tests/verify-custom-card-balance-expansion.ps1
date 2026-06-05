$ErrorActionPreference = 'Stop'

$root = Join-Path $PSScriptRoot '..'
$configuration = Get-Content -LiteralPath (Join-Path $root 'ModConfiguration.cs') -Raw -Encoding utf8
$settingsPanel = Get-Content -LiteralPath (Join-Path $root 'SettingsPanel.cs') -Raw -Encoding utf8
$patches = Get-Content -LiteralPath (Join-Path $root 'CardPatches.cs') -Raw -Encoding utf8
$installer = Get-Content -LiteralPath (Join-Path $root 'HarmonyPatchInstaller.cs') -Raw -Encoding utf8
$manifest = Get-Content -LiteralPath (Join-Path $root 'CustomCardBalance.json') -Raw -Encoding utf8
$project = Get-Content -LiteralPath (Join-Path $root 'CustomCardBalance.csproj') -Raw -Encoding utf8
$deployScript = Get-Content -LiteralPath (Join-Path $root 'deploy.bat') -Raw -Encoding utf8
$noFocusIcon = Join-Path $root 'assets\no_focus_gain_power.png'

function New-TextFromCodepoints {
    param([int[]] $Codepoints)
    return -join ($Codepoints | ForEach-Object { [char] $_ })
}

function Test-KnownMenuText {
    param(
        [string] $Source,
        [string] $Message
    )

    $expected = switch ($Message) {
        'Forgotten Ritual 2.3.0 menu text is missing.' {
            @(
                (New-TextFromCodepoints @(0x88AB,0x9057,0x5FD8,0x7684,0x4EEA,0x5F0F)),
                (New-TextFromCodepoints @(0x5347,0x7EA7,0x540E,0x79FB,0x9664,0x6D88,0x8017,0xFF0C,0x4F46,0x4E0D,0x589E,0x52A0,0x80FD,0x91CF))
            )
            break
        }
        'Spite 2.3.0 menu row is missing.' {
            @(
                (New-TextFromCodepoints @(0x6028,0x6068)),
                (New-TextFromCodepoints @(0x4F24,0x5BB3,0x6539,0x4E3A,0x62BD,0x724C))
            )
            break
        }
        'Wraith Form 2.3.0 menu row is missing.' {
            @(
                (New-TextFromCodepoints @(0x5E7D,0x9B42,0x5F62,0x6001)),
                (New-TextFromCodepoints @(0x5347,0x7EA7,0x540E,0x83B7,0x5F97,0x4FDD,0x7559,0xFF1B,0x8D1F,0x9762,0x6548,0x679C,0x524A,0x5F31))
            )
            break
        }
        'Biased Cognition 2.3.0 menu row is missing.' {
            @(
                (New-TextFromCodepoints @(0x504F,0x5DEE,0x8BA4,0x77E5)),
                (New-TextFromCodepoints @(0x8D1F,0x9762,0x6548,0x679C,0x524A,0x5F31))
            )
            break
        }
        'Hidden Gem 2.3.0 menu row is missing.' {
            @(
                (New-TextFromCodepoints @(0x672A,0x6398,0x5B9D,0x77F3)),
                (New-TextFromCodepoints @(0x968F,0x673A,0x724C,0x83B7,0x53D6,0x6548,0x679C))
            )
            break
        }
        default { $null }
    }

    if ($null -eq $expected) {
        return $false
    }

    foreach ($fragment in $expected) {
        if ($Source -notmatch [regex]::Escape($fragment)) {
            return $false
        }
    }

    return $true
}

function Assert-Contains {
    param(
        [string] $Source,
        [string] $Pattern,
        [string] $Message
    )

    if ($Source -notmatch $Pattern) {
        if (Test-KnownMenuText -Source $Source -Message $Message) {
            return
        }

        $fallbackPatterns = @{
            'Forgotten Ritual 2.3.0 menu text is missing.' = '被遗忘的仪式", "升级后移除消耗，但不增加能量"'
            'Spite 2.3.0 menu row is missing.' = '怨恨", "伤害改为抽牌"'
            'Wraith Form 2.3.0 menu row is missing.' = '幽魂形态", "升级后获得保留；负面效果削弱"'
            'Biased Cognition 2.3.0 menu row is missing.' = '偏差认知", "负面效果削弱"'
            'Hidden Gem 2.3.0 menu row is missing.' = '未掘宝石", "随机牌获取效果"'
        }

        if ($fallbackPatterns.ContainsKey($Message) -and $Source -match [regex]::Escape($fallbackPatterns[$Message])) {
            return
        }

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

Assert-Contains $manifest '"id": "CustomCardBalance"' 'The internal manifest ID is not CustomCardBalance.'
Assert-Contains $manifest '"name": "Custom Card Balance"' 'The public mod name is not Custom Card Balance.'
Assert-Contains $manifest '"author": "Bruiser"' 'The manifest author is not Bruiser.'
Assert-Contains $manifest '"version": "2\.3\.0"' 'The manifest version is not 2.3.0.'
Assert-Contains $manifest '26 ' 'The manifest description does not mention 26 configurable cards.'
Assert-Contains $manifest '"affects_gameplay": true' 'The manifest must remain gameplay-relevant for multiplayer mod checks.'
Assert-Contains $configuration 'Path\.Combine\(OS\.GetUserDataDir\(\), "CustomCardBalance", "settings\.json"\)' 'The active CustomCardBalance settings path is missing.'

$cardIds = @(
    'ForgottenRitual',
    'Spite',
    'Acrobatics',
    'Untouchable',
    'Anticipate',
    'Speedster',
    'WraithForm',
    'Voltaic',
    'Hotfix',
    'Defragment',
    'Coolant',
    'BiasedCognition',
    'Hailstorm',
    'Rainbow',
    'Glow',
    'Alignment',
    'VoidForm',
    'TheSealedThrone',
    'BansheesCry',
    'Dirge',
    'Seance',
    'BorrowedTime',
    'Debilitate',
    'Defy',
    'Production',
    'HiddenGem'
)

foreach ($cardId in $cardIds) {
    Assert-Contains $configuration "public const string $cardId = " "Missing card ID: $cardId"
    Assert-Contains $settingsPanel "CardIds\.$cardId" "Missing settings-panel row: $cardId"
}

$removedCardIds = @('Dominate', 'ExpectAFight', 'Murder')
foreach ($cardId in $removedCardIds) {
    Assert-DoesNotContain $configuration "public const string $cardId = " "Removed card ID is still registered: $cardId"
    Assert-DoesNotContain $settingsPanel "CardIds\.$cardId" "Removed card is still shown in the settings panel: $cardId"
}

$entryCount = ([regex]::Matches($settingsPanel, 'new CardBalanceEntry\(')).Count
if ($entryCount -ne 26) {
    throw "Expected 26 menu rows, found $entryCount."
}

Assert-Contains $settingsPanel '被遗忘的仪式", "升级后移除消耗，但不增加能量"' 'Forgotten Ritual 2.3.0 menu text is missing.'
Assert-Contains $settingsPanel '怨恨", "伤害改为抽牌"' 'Spite 2.3.0 menu row is missing.'
Assert-Contains $settingsPanel '幽魂形态", "升级后获得保留；负面效果削弱"' 'Wraith Form 2.3.0 menu row is missing.'
Assert-Contains $settingsPanel '偏差认知", "负面效果削弱"' 'Biased Cognition 2.3.0 menu row is missing.'
Assert-Contains $settingsPanel '未掘宝石", "随机牌获取效果"' 'Hidden Gem 2.3.0 menu row is missing.'

$requiredPatchClasses = @(
    'ForgottenRitualVarsPatch',
    'ForgottenRitualUpgradePatch',
    'SpiteVarsPatch',
    'SpitePlayPatch',
    'SpiteUpgradePatch',
    'UntouchableVarsPatch',
    'UntouchableUpgradePatch',
    'AnticipateVarsPatch',
    'AnticipateUpgradePatch',
    'SpeedsterUpgradePatch',
    'WraithFormVarsPatch',
    'WraithFormHoverTipsPatch',
    'WraithFormPlayPatch',
    'WraithFormUpgradePatch',
    'VoltaicUpgradePatch',
    'HotfixUpgradePatch',
    'DefragmentVarsPatch',
    'DefragmentUpgradePatch',
    'CoolantVarsPatch',
    'CoolantUpgradePatch',
    'BiasedCognitionVarsPatch',
    'BiasedCognitionHoverTipsPatch',
    'BiasedCognitionPlayPatch',
    'BiasedCognitionUpgradePatch',
    'BiasedCognitionPowerTurnStartPatch',
    'BiasedCognitionFocusGainPatch',
    'BiasedCognitionPowerStackTypePatch',
    'BiasedCognitionPowerFlashPatch',
    'HailstormVarsPatch',
    'HailstormHoverTipsPatch',
    'HailstormUpgradePatch',
    'DebilitateVarsPatch',
    'DebilitateUpgradePatch',
    'HiddenGemPlayPatch',
    'NoDexterityGainPower',
    'PowerIconPatch',
    'LocTableHasEntryPatch',
    'GameplayRelevantModNameListPatch'
)

foreach ($patchClass in $requiredPatchClasses) {
    Assert-Contains $patches "class $patchClass" "Missing patch class: $patchClass"
    if ($patchClass -match 'Patch$') {
        Assert-Contains $installer $patchClass "Patch installer does not register: $patchClass"
    }
}

Assert-Contains $patches 'DexterityPower' 'No-Dexterity-gain power does not target DexterityPower.'
Assert-Contains $patches 'TemporaryDexterityPower' 'No-Dexterity-gain power does not target temporary Dexterity sources.'
Assert-Contains $patches 'FocusPower' 'No-Focus-gain power does not target FocusPower.'
Assert-Contains $patches 'TemporaryFocusPower' 'No-Focus-gain power does not target temporary Focus sources.'
Assert-Contains $patches 'amount <= 0m' 'No-gain powers must allow negative Dexterity/Focus changes.'
Assert-Contains $patches 'PowerCmd\.Apply<NoDexterityGainPower>' 'Wraith Form does not apply the no-Dexterity-gain power.'
Assert-Contains $patches 'PowerCmd\.Apply<BiasedCognitionPower>' 'Biased Cognition must reuse the original BiasedCognitionPower icon.'
Assert-DoesNotContain $patches 'PowerCmd\.Apply<NoFocusGainPower>' 'Biased Cognition must not apply an extra no-Focus-gain power.'
Assert-DoesNotContain $patches 'NoFocusGainPower' 'The extra no-Focus-gain power should be removed.'
Assert-Contains $patches 'BIASED_COGNITION_POWER\.description' 'Biased Cognition power description override is missing.'
Assert-Contains $patches 'NO_DEXTERITY_GAIN_POWER\.smartDescription' 'No-Dexterity-gain smart description override is missing.'
Assert-Contains $patches 'LocManager\.Instance\.Language' 'Power localization overrides must use the active game language.'
Assert-Contains $patches 'Task\.CompletedTask' 'Skipped Biased Cognition turn-start logic must return a completed Task.'
Assert-Contains $patches 'PowerStackType\.None' 'Biased Cognition power should hide the obsolete counter amount.'
Assert-Contains $installer 'PatchPrefix\(harmony, typeof\(AbstractModel\), nameof\(AbstractModel\.AfterModifyingPowerAmountReceived\), typeof\(BiasedCognitionPowerFlashPatch\)' 'Biased Cognition focus block flash patch is not registered on the hook callback.'
Assert-Contains $patches 'AccessTools\.Method\(typeof\(PowerModel\), "Flash"\)' 'Biased Cognition focus block must trigger the original power flash animation.'
Assert-DoesNotContain $patches 'still works' 'Power descriptions should not mention loss still works.'
Assert-DoesNotContain $patches '仍会生效' 'Chinese power descriptions should not mention loss still works.'
Assert-Contains $patches 'AfterOrbEvoked' 'Hailstorm must trigger from Frost orb evocation.'
Assert-DoesNotContain $patches 'c\.GetEnchantedReplayCount\(\) < 1' 'Hidden Gem should no longer filter out cards that already have Replay.'
Assert-Contains $configuration 'GetMultiplayerCompatibilityToken' 'The multiplayer settings compatibility token is missing.'
Assert-Contains $configuration 'SHA256' 'The multiplayer settings token must use a stable hash.'
Assert-Contains $patches 'StartsWith\("CustomCardBalance-", StringComparison\.Ordinal\)' 'The gameplay-relevant mod list does not identify this mod entry.'
Assert-Contains $patches 'ModConfiguration\.GetMultiplayerCompatibilityToken\(\)' 'The gameplay-relevant mod list does not include the settings hash.'
Assert-Contains $patches 'no_dexterity_gain_power\.png' 'The no-Dexterity-gain icon asset is not referenced.'
Assert-DoesNotContain $patches 'no_focus_gain_power\.png' 'Biased Cognition should not reference a custom no-Focus icon asset.'
Assert-Contains $patches 'ImageTexture\.CreateFromImage' 'Custom power icons are not loaded as runtime ImageTextures.'
Assert-Contains $installer 'PatchPostfix\(harmony, typeof\(LocTable\), nameof\(LocTable\.HasEntry\)' 'The custom power localization existence patch is missing.'

if (Test-Path -LiteralPath $noFocusIcon) {
    throw 'The obsolete no-Focus-gain icon asset should be removed.'
}

Assert-Contains $project 'assets\\\*\.png' 'The project does not copy icon assets to build output.'
Assert-Contains $deployScript 'assets' 'The deploy script does not copy icon assets.'

Write-Host 'Custom Card Balance 2.3.0 regression checks passed.'
