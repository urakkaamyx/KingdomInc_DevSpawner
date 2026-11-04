using Assets.Source.Player;
using Assets.Behaviour.Flow;
using Assets.Source.Flow.Elements;
using BepInEx;
using HarmonyLib;

namespace KingdomDevSpawner
{
    [BepInPlugin(Id, "DevSpawner", "1.1.0")]
    public sealed partial class DevSpawnerPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.devspawner";
        Harmony patches;

        void Awake()
        {
            patches = new Harmony(Id);
            patches.PatchAll(typeof(DevSpawnerPlugin).Assembly);
            Logger.LogInfo("DevSpawner enabled: Spawner is available in the Resource build menu.");
#if DEVSPAWNER_DIAGNOSTICS
            if (Diagnostic) { UnityEngine.Application.runInBackground = true; StartCoroutine(Verify()); }
#endif
        }

        void OnDestroy() => patches?.UnpatchSelf();

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
        // Leave the hidden tech and the player's saved research data intact.
        [HarmonyPatch(typeof(GamePlayer), nameof(GamePlayer.HasTech))]
        static class EnableSpawner
        {
            static bool Prefix(TechNode tech, ref bool __result)
            {
                if (tech?.Identifier != "dev_item_spawner") return true;
                __result = true;
                return false;
            }
        }
    }
}
