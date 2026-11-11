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
        // Existing kingdoms with placed Spawners retain access when upgrading.
        [HarmonyPatch(typeof(GamePlayer), nameof(GamePlayer.FromJson))]
        static partial class ExistingSpawners
        {
            static void Postfix(GamePlayer __result)
            {
                if (__result != null && !__result.HasTech(SpawnerResearch)
                    && __result.Flows.Any(flow => flow.Elements.Any(element => element is DevSpawner)))
                    __result.AddTech(SpawnerResearch);
            }
        }
    }
}
