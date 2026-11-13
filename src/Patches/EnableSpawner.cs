using BepInEx;
using System.Linq;
using KingdomFoundation.Api;

namespace KingdomDevSpawner
{
    internal // Both the native build menu and native spawning behavior use this gate.
    // Bridge the hardcoded production gate to the Foundation research purchase.
    static class EnableSpawner
    {
        internal static bool Prefix(GameKingdom __instance, GameResearchNode tech, ref bool __result)
        {
            if (tech?.Identifier != "dev_item_spawner")
                return true;
            __result = __instance.HasResearch(global::KingdomDevSpawner.DevSpawnerPlugin.SpawnerResearch);
            return false;
        }
    }
}
