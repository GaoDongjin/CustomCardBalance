# Custom Card Balance

Custom Card Balance is a configurable card-balance mod for **Slay the Spire 2**.
It lets players enable or disable each adjustment independently, so disabled
cards keep the current installed game's vanilla behavior.

## Compatibility

- Mod version: `2.3.0`
- Supported game version: `0.106.1`
- Author: `Bruiser`
- BaseLib dependency: not required
- Multiplayer: supported only when every player has this mod installed and all
  players use the same card-toggle settings

This project targets the `Ver0.106` game codebase. Compatibility with newer
game versions is not implied.

## Features

- Adjusts 26 cards across all five characters and the shared card pool.
- Adds an in-game settings panel with per-card toggles.
- Opens the panel from the main menu with `F1`.
- Adds a fallback entry to the game's Settings menu.
- Preserves vanilla card behavior when an individual adjustment is disabled.
- Saves settings to the Godot user-data directory and exits the game so the
  configuration takes effect after restart.
- Adds gameplay-relevant multiplayer compatibility tokens based on the current
  toggle settings.
- Migrates settings from the previous `RevertCardsMod` configuration directory
  on first launch.
- Builds the settings UI only when it is first opened, keeping startup work
  minimal.

## Installation

1. Download the latest release archive.
2. Extract the `CustomCardBalance` folder into the game's `mods` directory.
3. Remove any previous `RevertCardsMod` folder from the same directory.
4. Launch the game.

The final folder structure should look like this:

```text
Slay the Spire 2/
+-- mods/
    +-- CustomCardBalance/
        +-- CustomCardBalance.dll
        +-- CustomCardBalance.json
        +-- assets/
```

## Usage

1. Return to the main menu.
2. Press `F1`, or open `Custom Card Balance` from the game's Settings menu.
3. Enable or disable individual card adjustments.
4. Select `Save and Exit`.
5. Restart the game.

Disabled adjustments always fall back to the currently installed game's
vanilla card behavior.

## Adjusted Cards

The mod currently includes 26 independently configurable card adjustments:

| Pool | Cards |
| --- | --- |
| Ironclad | Forgotten Ritual, Spite |
| Silent | Acrobatics, Untouchable, Anticipate, Speedster, Wraith Form |
| Defect | Voltaic, Hotfix, Defragment, Coolant, Biased Cognition, Hailstorm, Rainbow |
| Regent | Glow, Alignment, Void Form, The Sealed Throne |
| Necrobinder | Banshee's Cry, Dirge, Seance, Borrowed Time, Debilitate, Defy |
| Shared pool | Production, Hidden Gem |

## Version 2.3.0 Highlights

| Area | Change |
| --- | --- |
| Card list | Expanded from 21 to 26 configurable adjustments. |
| Added adjustments | Spite, Hotfix, Defragment, Coolant, Biased Cognition, Hailstorm, Debilitate, Hidden Gem. |
| Removed adjustments | Dominate, Expect a Fight, and Murder are no longer modified. |
| Wraith Form | Reworked around Intangible plus a combat-long block on positive Dexterity gain, with a dedicated crossed-out Dexterity icon. |
| Biased Cognition | Reuses the original Biased Cognition power icon, removes the recurring Focus loss, blocks positive Focus gain, hides the obsolete stack number, and flashes when Focus gain is prevented. |
| Hailstorm | Triggers from Frost orb evocation instead of only passive turn timing. |
| Hidden Gem | Uses the Ver0.100-style random card targeting logic and no longer filters out cards that already have Replay. |
| Multiplayer | Includes the complete card-toggle configuration in the gameplay-relevant compatibility token. |

See [`SPEC.md`](SPEC.md) for the complete Ver0.106 baseline table.

## Building from Source

Requirements:

- .NET SDK 9
- A local installation of Slay the Spire 2

Set `STS2_GAME_DIR` to the game installation directory before building:

```powershell
$env:STS2_GAME_DIR = 'C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2'
dotnet build .\CustomCardBalance.csproj -c Release --no-restore
```

Run the static regression checks:

```powershell
powershell -ExecutionPolicy Bypass -File .\tests\verify-custom-card-balance-rename-and-lazy-ui.ps1
powershell -ExecutionPolicy Bypass -File .\tests\verify-custom-card-balance-expansion.ps1
powershell -ExecutionPolicy Bypass -File .\tests\verify-hotkey-entry.ps1
powershell -ExecutionPolicy Bypass -File .\tests\verify-settings-layout.ps1
powershell -ExecutionPolicy Bypass -File .\tests\verify-startup-patching.ps1
```

Create a release archive:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\package-release.ps1
```

## License

This project is released under the [MIT License](LICENSE).

## Disclaimer

This is an unofficial community mod. It is not affiliated with or endorsed by
Mega Crit.
