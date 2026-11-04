DevSpawner 1.1.0
By rockyamyx / Urakka

Enables Kingdom Inc's built-in Spawner as a cheat node. It generates items
without normal production inputs and displays the produced item's name and
icon, with a hover description.

REQUIREMENTS
Kingdom Inc for Windows x64, Unity Mono build.
BepInEx 5, tested with 5.4.23.6 and its bundled Harmony:
https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6
BepInEx is installed separately. No other gameplay mod is required.

INSTALL
1. Close Kingdom Inc.
2. Install BepInEx 5 for Windows x64 beside KingdomInc.exe if needed.
3. Extract this ZIP into that same game folder.
4. Confirm: BepInEx/plugins/DevSpawner/DevSpawner.dll
5. Start the game and open the Resource build menu. Build Spawner.

USE
Connect the Spawner's output to an input that accepts one specific item, such
as an ingredient input on a configured crafting node. The Spawner learns that
item and generates it automatically. Its icon and bottom caption identify the
actual item being produced. Hover for the description.

An unset node displays "Connect an item input". Inputs that accept many item
types without naming one cannot choose the item for it. Normal node purchase
costs still apply. There is no separate research requirement while enabled.

The native Spawner remembers its first selected item, including across saves
and after reconnecting it. To choose a different item, build a fresh Spawner.

UPDATE OR REMOVE
Close the game before replacing or deleting this DLL. To remove, delete
BepInEx/plugins/DevSpawner/DevSpawner.dll. No custom save fields or node types
are added. Native Spawners retain their saved item, but stop producing without
this mod unless another mod or developer unlock enables the same technology.
Removing the plugin does not undo items that it already produced.

COMPATIBILITY
The mod enables only dev_item_spawner; other research remains unchanged.
Changes to the same technology gate or Spawner visual behavior may conflict.
Passed 18 isolated Unity runtime checks for native catalog/prefab availability,
spawning, saved-item roundtrip, connection selection and rendered item display.
Manual installation is verified. Vortex deployment and full companion-plugin
compatibility were not tested. Game updates may require a mod update.

No game, BepInEx or Harmony binaries are included in this package.
Promotional thumbnail: AI-generated edit of the shared Kingdom Inc mod artwork.
The gallery preview is a render of actual native prefabs in the diagnostic
runtime, not a screenshot of a played kingdom.

TROUBLESHOOTING
Check BepInEx/LogOutput.log for DevSpawner 1.1.0 and the enabled message. Include
game/mod versions, the log and the connected input's item in bug reports.
