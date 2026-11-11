# DevSpawner

[GitHub releases](https://github.com/urakkaamyx/KingdomInc_DevSpawner/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/13) · Version 1.3.0

Enable Kingdom Inc's native Spawner building to spawn any item without production inputs. This is a cheat utility designed for testing, sandbox experimentation, and blueprint design.

## Features

- **Free Research Unlock:** Unlock DevSpawner for free in **Tech Tree > Mods**, then construct Spawners from the **Resource** build menu.
- **Searchable Item Picker:** Click the **+** or the active item icon on any Spawner node to open an in-game search picker.
- **Full Item Catalog:** Select any item in the game, including late-game and locked items, with names, icons, and base gold values displayed.
- **Dynamic Node Visuals:** The chosen item's native icon and name render directly on the Spawner node. Hovering displays its base gold value.
- **Persistent Selection:** Spawner configurations and selected items survive saving and reloading.
- **Auto-Learning Input:** An unset Spawner can also learn what item to spawn from a connected pipe input.

## Usage and Controls

1. Open **Tech Tree > Mods** and research **DevSpawner** for free.
2. Build a **Spawner** from the **Resource** menu (normal building placement costs apply).
3. Click the **+** button on the Spawner to open the item picker.
4. Search by item name or ID and click a row to choose what it produces.
5. Click the selected item's icon on the node at any time to change items.

*Note: Changing the active item clears any pending output while keeping connected pipes intact.*

## Requirements

- Kingdom Inc (Windows x64 Mono)
- [BepInEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6) (5.4.23.6 or newer)
- [Foundation](https://www.nexusmods.com/kingdominc/mods/18) (0.11.0 or newer)

## Installation

1. Install BepInEx 5 for Kingdom Inc.
2. Install Foundation (0.11.0 or newer).
3. Extract the release ZIP into your Kingdom Inc game directory.
4. Verify the file exists at `BepInEx/plugins/DevSpawner/DevSpawner.dll`.
5. Launch the game and unlock DevSpawner in Tech Tree > Mods!

## Compatibility

| Class.Method | Hook Type | Impact |
|---|---|---|
| GamePlayer.HasTech | Harmony Postfix | Unlocks the native Spawner building when the DevSpawner research is completed. |
| GamePlayer.FromJson | Harmony Postfix | Ensures existing placed Spawners retain their functional unlocks across loaded saves. |
| WorldFlowElement.SetElement | Harmony Postfix | Attaches interactive item selection UI, item icons, and tooltips to placed Spawner nodes. |
| DevSpawner.IconName (getter) | Harmony Postfix | Displays the selected item's icon on the Spawner node dynamically. |
| DevSpawner..ctor | Harmony Postfix | Links the Spawner building prerequisite to the modded DevSpawner tech. |

## Removal

Close the game and delete `BepInEx/plugins/DevSpawner/`. Existing Spawners will retain their saved item reference but cease spawning.

## Credits and Source

Created by **Urakka** (rockyamyx). [Source code on GitHub](https://github.com/urakkaamyx/KingdomInc_DevSpawner). Game artwork belongs to its respective owners.
