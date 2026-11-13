using BepInEx;
using System.Linq;
using KingdomFoundation.Api;

namespace KingdomDevSpawner
{
    internal // Keep the game's native node type and save data; only change its build prerequisite.
    static class SpawnerPrerequisite
    {
        internal static void Postfix(WorkspaceObject __instance) => __instance.RequiredResearch = global::KingdomDevSpawner.DevSpawnerPlugin.SpawnerResearch;
    }
}
