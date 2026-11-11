using Assets.Source.Player;
using Assets.Behaviour.Flow;
using Assets.Source.Flow.Elements;
using BepInEx;
using HarmonyLib;
using System.Linq;
using KingdomFoundation.Api;

namespace KingdomDevSpawner
{
    public sealed partial class DevSpawnerPlugin : BaseUnityPlugin
    {
        // Keep the correct item icon when native code refreshes the node visuals.
        [HarmonyPatch(typeof(DevSpawner), nameof(DevSpawner.IconName), MethodType.Getter)]
        static partial class SpawnedItemIcon
        {
            static void Postfix(DevSpawner __instance, ref string __result)
            {
                if (__instance.Item != null) __result = __instance.Item.IconName;
            }
        }
    }
}
