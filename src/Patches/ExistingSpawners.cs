using BepInEx;
using System.Linq;
using KingdomFoundation.Api;

namespace KingdomDevSpawner
{
    internal // Existing kingdoms with placed Spawners retain access when upgrading.
    static class ExistingSpawners
    {
        internal static void Postfix(GameKingdom __result)
        {
            if (__result != null && !__result.HasResearch(global::KingdomDevSpawner.DevSpawnerPlugin.SpawnerResearch) && __result.Buildings.Any(element => element.IsSpawner))
                __result.UnlockResearch(global::KingdomDevSpawner.DevSpawnerPlugin.SpawnerResearch);
        }
    }
}
