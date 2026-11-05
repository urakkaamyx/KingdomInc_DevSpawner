using Assets.Source.Player;
using Assets.Behaviour.Flow;
using Assets.Source.Flow.Elements;
using BepInEx;
using HarmonyLib;
using System.Linq;
using KingdomFoundation.Api;

namespace KingdomDevSpawner
{
    [BepInPlugin(Id, "DevSpawner", "1.3.0")]
    [BepInDependency("urakka.kingdominc.foundation", "0.5.0")]
    public sealed partial class DevSpawnerPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.devspawner";
        internal const string ResearchId = Id + ".unlock";
        Harmony patches;
        ITechNode research;
        internal static TechNode SpawnerResearch => TechNode.Get(ResearchId);

        void Awake()
        {
            try
            {
                research = FoundationApi.CreateTechNode(new TechNodeDefinition(ResearchId, "DevSpawner",
                    new UnityEngine.Vector2(440, 0),
                    "Unlock the Spawner. Choose any item with its + button and see its gold value.",
                    research: new TechNodeOptions(fixedCost: new CurrencyCollection(), iconName: "Icons_18")));
                patches = new Harmony(Id);
                patches.PatchAll(typeof(DevSpawnerPlugin).Assembly);
            }
            catch { patches?.UnpatchSelf(); research?.Dispose(); throw; }
            Logger.LogInfo("DevSpawner ready: unlock it for free in Tech Tree > Mods, then build Spawner from Resource.");
#if DEVSPAWNER_DIAGNOSTICS
            if (Diagnostic) { UnityEngine.Application.runInBackground = true; StartCoroutine(Verify()); }
#endif
        }

        void OnDestroy() { patches?.UnpatchSelf(); research?.Dispose(); }

        // Keep the game's native node type and save data; only change its build prerequisite.
        [HarmonyPatch(typeof(DevSpawner), MethodType.Constructor)]
        static class SpawnerPrerequisite
        {
            static void Postfix(DevSpawner __instance) => __instance.RequiredTech = SpawnerResearch;
        }

        // Existing kingdoms with placed Spawners retain access when upgrading.
        [HarmonyPatch(typeof(GamePlayer), nameof(GamePlayer.FromJson))]
        static class ExistingSpawners
        {
            static void Postfix(GamePlayer __result)
            {
                if (__result != null && !__result.HasTech(SpawnerResearch)
                    && __result.Flows.Any(flow => flow.Elements.Any(element => element is DevSpawner)))
                    __result.AddTech(SpawnerResearch);
            }
        }

        [HarmonyPatch(typeof(WorldFlowElement), nameof(WorldFlowElement.SetElement))]
        static class ShowSpawnedItem
        {
            static void Postfix(WorldFlowElement __instance)
            {
                if (!(__instance.Element is DevSpawner)) return;
                var display = __instance.GetComponent<SpawnerDisplay>() ?? __instance.gameObject.AddComponent<SpawnerDisplay>();
                display.Refresh();
            }
        }

        // Keep the correct item icon when native code refreshes the node visuals.
        [HarmonyPatch(typeof(DevSpawner), nameof(DevSpawner.IconName), MethodType.Getter)]
        static class SpawnedItemIcon
        {
            static void Postfix(DevSpawner __instance, ref string __result)
            {
                if (__instance.Item != null) __result = __instance.Item.IconName;
            }
        }

        // Both the native build menu and native spawning behavior use this gate.
        // Bridge the hardcoded production gate to the Foundation research purchase.
        [HarmonyPatch(typeof(GamePlayer), nameof(GamePlayer.HasTech))]
        static class EnableSpawner
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
