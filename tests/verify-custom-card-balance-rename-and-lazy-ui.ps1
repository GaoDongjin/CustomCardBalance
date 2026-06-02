$ErrorActionPreference = 'Stop'

$root = Join-Path $PSScriptRoot '..'

function Read-RequiredFile {
    param([string] $RelativePath)

    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing required file: $RelativePath"
    }

    return Get-Content -LiteralPath $path -Raw -Encoding utf8
}

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

$manifest = Read-RequiredFile 'CustomCardBalance.json'
$project = Read-RequiredFile 'CustomCardBalance.csproj'
$plugin = Read-RequiredFile 'Plugin.cs'
$configuration = Read-RequiredFile 'ModConfiguration.cs'
$settingsPanel = Read-RequiredFile 'SettingsPanel.cs'
$deployScript = Read-RequiredFile 'deploy.bat'
$allSource = ($plugin, $configuration, $settingsPanel, (Read-RequiredFile 'CardPatches.cs')) -join "`n"

Assert-Contains $manifest '"id": "CustomCardBalance"' 'The manifest ID was not renamed to CustomCardBalance.'
Assert-Contains $manifest '"name": "Custom Card Balance"' 'The public mod name was not renamed to Custom Card Balance.'
Assert-Contains $manifest '"author": "Bruiser"' 'The manifest author was not set to Bruiser.'
Assert-Contains $manifest '"version": "2\.2\.0"' 'The manifest version was not increased to 2.2.0.'
Assert-Contains $project '<AssemblyName>CustomCardBalance</AssemblyName>' 'The DLL assembly name was not renamed.'
Assert-Contains $project '<RootNamespace>CustomCardBalance</RootNamespace>' 'The root namespace was not renamed.'
Assert-Contains $allSource 'namespace CustomCardBalance;' 'The C# namespace was not renamed.'
Assert-DoesNotContain $allSource 'namespace RevertCardsMod;' 'The legacy namespace is still present.'
Assert-Contains $plugin 'com\.bruiser\.customcardbalance' 'The Harmony ID was not renamed.'
Assert-Contains $plugin 'Initialization completed in' 'The mod initializer timing log is missing.'

Assert-Contains $configuration 'Path\.Combine\(OS\.GetUserDataDir\(\), "CustomCardBalance", "settings\.json"\)' 'The active settings path was not renamed.'
Assert-Contains $configuration 'Path\.Combine\(OS\.GetUserDataDir\(\), "RevertCardsMod", "settings\.json"\)' 'The legacy settings path is missing.'
Assert-Contains $configuration 'MigrateLegacySettingsIfNeeded\(\)' 'The configuration loader does not migrate legacy settings.'

Assert-Contains $settingsPanel 'private const string SettingsRowName = "CustomCardBalanceSettings";' 'The settings row node name was not renamed.'
Assert-Contains $settingsPanel 'row\.GetNode<RichTextLabel>\("Label"\)\.Text = "Custom Card Balance";' 'The settings-menu label was not renamed.'
Assert-Contains $settingsPanel 'Label\("Custom Card Balance ' 'The panel title was not renamed.'

$initializeMatch = [regex]::Match(
    $settingsPanel,
    'public void Initialize\(NMainMenu mainMenu\)\s*\{(?<body>[\s\S]*?)\n    \}'
)
if (-not $initializeMatch.Success) {
    throw 'Could not inspect CardBalanceSettingsPanel.Initialize.'
}
Assert-DoesNotContain $initializeMatch.Groups['body'].Value 'BuildUi\(\)|RenderRows\(\)' 'Initialize still eagerly builds the full UI during main-menu startup.'
Assert-Contains $settingsPanel 'private void EnsureUiBuilt\(\)' 'The lazy UI builder is missing.'
Assert-Contains $settingsPanel 'EnsureUiBuilt\(\);[\s\S]*?SetPanelVisible\(true\);' 'The panel is not built when opened from the settings menu.'
Assert-Contains $settingsPanel 'EnsureUiBuilt\(\);[\s\S]*?Visible = visible;' 'The panel is not built when opened from the F1 hotkey.'
Assert-Contains $settingsPanel 'Settings panel shell registered in' 'The main-menu shell timing log is missing.'
Assert-Contains $settingsPanel 'Settings UI built on demand in' 'The on-demand UI timing log is missing.'

Assert-Contains $deployScript 'set MOD_ID=CustomCardBalance' 'The deployment directory ID was not renamed.'
Assert-Contains $deployScript 'set PROJECT_NAME=CustomCardBalance' 'The deployment artifact name was not renamed.'

Write-Host 'Custom Card Balance rename and lazy UI checks passed.'
