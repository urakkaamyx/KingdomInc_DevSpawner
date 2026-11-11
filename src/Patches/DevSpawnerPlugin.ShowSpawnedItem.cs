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
        [HarmonyPatch(typeof(WorldFlowElement), nameof(WorldFlowElement.SetElement))]
        static partial class ShowSpawnedItem
        {
            static void Postfix(WorldFlowElement __instance)
            {
                if (!(__instance.Element is DevSpawner)) return;
                var display = __instance.GetComponent<SpawnerDisplay>() ?? __instance.gameObject.AddComponent<SpawnerDisplay>();
                display.Refresh();
            }
        }
    }
}
