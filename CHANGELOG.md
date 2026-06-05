# Changelog

## 2.3.0

| Area | Change |
| --- | --- |
| Card list | Expanded the configurable adjustment list from 21 to 26 cards. |
| Added | Spite, Hotfix, Defragment, Coolant, Biased Cognition, Hailstorm, Debilitate, and Hidden Gem. |
| Removed | Dominate, Expect a Fight, and Murder were removed from the active adjustment list. |
| Wraith Form | Added a combat-long positive Dexterity gain block and a dedicated crossed-out Dexterity icon. |
| Biased Cognition | Reused the original power icon, removed recurring Focus loss, blocked positive Focus gain, hid the obsolete stack number, and added flash feedback when Focus gain is prevented. |
| Hailstorm | Added Frost-orb-evoke trigger behavior. |
| Hidden Gem | Switched to the Ver0.100-style targeting behavior and allows cards that already have Replay. |
| Multiplayer | Added gameplay-relevant compatibility checks based on the full card-toggle settings hash. |
| Packaging | Added icon assets to the release output and deployment script. |

## 2.2.0

- Renamed the mod to `Custom Card Balance`.
- Changed the internal mod ID and assembly name to `CustomCardBalance`.
- Added automatic migration from the previous `RevertCardsMod` settings path.
- Added `Bruiser` as the manifest author.
- Deferred full settings-table construction until the panel is first opened.
- Added startup timing logs for initialization and settings UI construction.

## 2.1.0

- Expanded the configurable card list from 9 to 21 cards.
- Added the `Custom Balance Pack` settings panel with per-card toggles.
