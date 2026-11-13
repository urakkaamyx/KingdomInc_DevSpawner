using BepInEx;
using System.Linq;
using KingdomFoundation.Api;

namespace KingdomDevSpawner
{
    internal // Keep the correct item icon when native code refreshes the node visuals.
    static class SpawnedItemIcon
    {
        internal static void Postfix(WorkspaceObject __instance, ref string __result)
        {
            if (__instance.SpawnedItem != null)
                __result = __instance.SpawnedItem.IconName;
        }
    }
}
