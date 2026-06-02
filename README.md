# Custom Card Balance

Custom Card Balance is a configurable card-balance mod for **Slay the Spire 2**.
It provides optional adjustments for 21 cards while allowing players to enable
or disable each adjustment independently.

## Compatibility

- Mod version: `2.2.0`
- Supported game version: `0.106.1`
- Author: `Bruiser`
- BaseLib dependency: not required

This project targets the `Ver0.106` game codebase. Compatibility with newer
game versions is not implied.

## Features

- Adjusts 21 cards across all five characters and the shared card pool.
- Adds an in-game settings panel with per-card toggles.
- Opens the panel from the main menu with `F1`.
- Adds a fallback entry to the game's Settings menu.
- Preserves the current game's vanilla behavior when a card adjustment is
  disabled.
- Saves settings to the Godot user-data directory and exits the game so the
  new configuration can take effect after restart.
- Migrates settings from the previous `RevertCardsMod` configuration directory
  on first launch.
- Builds the settings UI only when it is first opened, keeping the full table
  out of the main-menu startup path.

## Installation

1. Download the latest release archive.
2. Extract the `CustomCardBalance` folder into the game's `mods` directory.
3. Remove any previous `RevertCardsMod` folder from the same directory.
4. Launch the game.

The final folder structure should look like this:

```text
Slay the Spire 2/
└── mods/
    └── CustomCardBalance/
        ├── CustomCardBalance.dll
        └── CustomCardBalance.json
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

The mod currently includes 21 independently configurable card adjustments:

- Ironclad: 3 cards
- Silent: 6 cards
- Defect: 2 cards
- Regent: 4 cards
- Necrobinder: 5 cards
- Shared pool: 1 card

The list contains 16 adjustments that restore Ver0.100 behavior, 3 custom
buffs, and 2 hybrid buffs that combine selected Ver0.100 and Ver0.106 behavior.

| Character | Card | Type | Adjustment |
| --- | --- | --- | --- |
| Ironclad | Dominate | Custom hybrid buff | Applies 1 Vulnerable. The upgrade removes Exhaust instead of applying additional Vulnerable. |
| Ironclad | Expect a Fight | Ver0.100 revert | Removes the energy-gain lock after use. The upgrade still reduces the energy cost. |
| Ironclad | Forgotten Ritual | Ver0.100 revert | Removes Exhaust. |
| Silent | Acrobatics | Ver0.100 revert | Changes rarity from Uncommon to Common. |
| Silent | Untouchable | Ver0.100 revert | Changes Block from 6/8 to 9/12. |
| Silent | Anticipate | Ver0.100 revert | Changes temporary Dexterity from 2/3 to 3/5. |
| Silent | Speedster | Custom hybrid buff | The upgrade grants both +1 trigger damage and Innate. |
| Silent | Murder | Custom buff | The upgrade retains its cost reduction and increases the per-draw extra damage multiplier from 1 to 2. |
| Silent | Wraith Form | Custom buff | Removes the Dexterity-loss effect. Grants 2 Intangible, or 3 Intangible and Retain when upgraded. |
| Defect | Voltaic | Ver0.100 revert | Reduces energy cost from 3 to 2. |
| Defect | Rainbow | Custom buff | Reduces energy cost from 2 to 1. |
| Regent | Glow | Ver0.100 revert | Draws 2 cards immediately and removes the next-turn draw effect. |
| Regent | Alignment | Ver0.100 revert | Reduces star cost from 3 to 2. |
| Regent | Void Form | Ver0.100 revert | Removes Ethereal. The upgrade increases the effect from 2 cards to 3 cards. |
| Regent | The Sealed Throne | Ver0.100 revert | The upgrade grants Innate instead of reducing energy cost. |
| Necrobinder | Banshee's Cry | Ver0.100 revert | Reduces energy cost from 9 to 6. The upgrade increases damage from 33 to 39 instead of reducing cost. |
| Necrobinder | Dirge | Ver0.100 revert | Removes Exhaust. |
| Necrobinder | Seance | Ver0.100 revert | Reduces energy cost from 1 to 0. The upgraded card transforms the selected card into an upgraded Soul. |
| Necrobinder | Borrowed Time | Ver0.100 revert | Costs 0. Applies 3 Doom to yourself and grants 1 energy, or 2 energy when upgraded, without increasing hand costs. |
| Necrobinder | Defy | Ver0.100 revert | The upgrade grants +1 Block and +1 Weak instead of +3 Block. |
| Shared pool | Production | Ver0.100 revert | The upgrade removes Exhaust instead of granting +1 energy. |

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
