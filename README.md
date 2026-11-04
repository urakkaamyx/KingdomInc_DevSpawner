# DevSpawner

[Nexus Mods release](https://www.nexusmods.com/kingdominc/mods/13)

Enables Kingdom Inc's built-in **Spawner** in the **Resource** build menu, including existing kingdoms. Connect its output to an input that accepts a specific item; the native node learns that item and generates it automatically. Ordinary node purchase costs still apply.

Version 1.1.0 shows the generated item's icon, a name below it, and a hover description. An unset Spawner displays **Connect an item input**. The display follows the actual saved spawning type; the native node keeps that type when disconnected or reconnected.

Requires the Mono build of Kingdom Inc and BepInEx 5.x. Installed at `BepInEx/plugins/DevSpawner/DevSpawner.dll`. Restart the game after installation.

Build from the game directory with `dotnet build _Mods/DevSpawner/DevSpawner.csproj -c Release`.

To disable, close the game and remove that DLL. Spawner nodes use the game's native save format and stop generating while the developer technology is unavailable. This plugin does not write research unlocks or enable other developer technologies.

Source repository: https://github.com/urakkaamyx/KingdomInc_DevSpawner
