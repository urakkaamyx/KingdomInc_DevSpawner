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
        // Both the native build menu and native spawning behavior use this gate.
        // Bridge the hardcoded production gate to the Foundation research purchase.
        [HarmonyPatch(typeof(GamePlayer), nameof(GamePlayer.HasTech))]
        static partial class EnableSpawner
        {
            static bool Prefix(GamePlayer __instance, TechNode tech, ref bool __result)
            {
                if (tech?.Identifier != "dev_item_spawner") return true;
                __result = __instance.HasTech(SpawnerResearch);
                return false;
            }
        }
    }
}
