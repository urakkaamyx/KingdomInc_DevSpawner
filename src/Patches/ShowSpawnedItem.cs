using BepInEx;
using System.Linq;
using KingdomFoundation.Api;

namespace KingdomDevSpawner
{
    internal static class ShowSpawnedItem
    {
        internal static void Postfix(UnityEngine.Behaviour __instance)
        {
            if (FoundationApi.WorkspaceAccess.FromView(__instance)?.IsSpawner != true)
                return;
            var display = __instance.GetComponent<SpawnerDisplay>() ?? __instance.gameObject.AddComponent<SpawnerDisplay>();
            display.Refresh();
        }
    }
}
