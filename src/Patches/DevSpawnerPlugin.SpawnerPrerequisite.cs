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
        // Keep the game's native node type and save data; only change its build prerequisite.
        [HarmonyPatch(typeof(DevSpawner), MethodType.Constructor)]
        static partial class SpawnerPrerequisite
        {
            static void Postfix(DevSpawner __instance) => __instance.RequiredTech = SpawnerResearch;
        }
    }
}
