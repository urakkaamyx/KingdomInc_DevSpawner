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
    [BepInDependency("urakka.kingdominc.foundation")]
    public sealed partial class DevSpawnerPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.devspawner";
        internal const string ResearchId = Id + ".unlock";
        Harmony patches;
        FoundationModContext modContext;
        internal static TechNode SpawnerResearch => TechNode.Get(ResearchId);


        void Awake()
        {
            try
            {
                modContext = FoundationApi.ForMod(Id);
                modContext.Register(new TechNodeDefinition(ResearchId, "DevSpawner", Id, "DevSpawner",
                    new UnityEngine.Vector2(440, 0),
                    "Unlock the Spawner. Choose any item with its + button and see its gold value.",
                    research: new TechNodeOptions(fixedCost: new CurrencyCollection(), iconName: "Icons_18")));
                patches = new Harmony(Id);
                patches.PatchAll(typeof(DevSpawnerPlugin).Assembly);
            }
            catch { patches?.UnpatchSelf(); modContext?.Dispose(); throw; }
            Logger.LogInfo("DevSpawner ready: unlock it for free in Tech Tree > Mods, then build Spawner from Resource.");
        }
    }
}
