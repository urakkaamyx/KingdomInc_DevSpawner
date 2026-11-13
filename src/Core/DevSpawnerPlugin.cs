using BepInEx;
using System.Linq;
using KingdomFoundation.Api;

namespace KingdomDevSpawner
{
    [BepInPlugin(Id, "DevSpawner", "1.3.1")]
    [BepInDependency("urakka.kingdominc.foundation", "0.14.0")]
    public sealed class DevSpawnerPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.devspawner";
        internal const string ResearchId = Id + ".unlock";
        readonly System.Collections.Generic.List<System.IDisposable> patches = new System.Collections.Generic.List<System.IDisposable>();
        FoundationModContext modContext;
        internal static GameResearchNode SpawnerResearch => FoundationApi.ResearchAccess.Find(ResearchId);

        void Awake()
        {
            try
            {
                modContext = FoundationApi.ForMod(Id);
                modContext.Register(new TechNodeDefinition(ResearchId, "DevSpawner", Id, "DevSpawner", new UnityEngine.Vector2(440, 0), "Unlock the Spawner. Choose any item with its + button and see its gold value.", research: TechNodeOptions.Create(fixedCost: new GameMoney(), iconName: "Icons_18")));
                RegisterHooks();
            }
            catch
            {
                DisposeHooks();
                modContext?.Dispose();
                throw;
            }

            Logger.LogInfo("DevSpawner ready: unlock it for free in Tech Tree > Mods, then build Spawner from Resource.");
        }

        private void RegisterHooks()
        {
            patches.Add(FoundationApi.GameHooks.SubscribeFiltered(Id, GameHookPoints.PlayerHasResearch, new GameHookFilter(argumentId: "dev_item_spawner"), before: call =>
            {
                bool result = false;
                if (!global::KingdomDevSpawner.EnableSpawner.Prefix((GameKingdom)call.Subject, call.Argument<GameResearchNode>(0), ref result))
                {
                    call.SetResult(result);
                    call.Cancel();
                }
            }));
            patches.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.KingdomDeserialized, after: call => global::KingdomDevSpawner.ExistingSpawners.Postfix(call.Result<GameKingdom>())));
            patches.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BuildingBound, after: call => global::KingdomDevSpawner.ShowSpawnedItem.Postfix(call.View as UnityEngine.Behaviour)));
            patches.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.SpawnerConstructed, after: call => global::KingdomDevSpawner.SpawnerPrerequisite.Postfix((WorkspaceObject)call.Subject)));
            patches.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.SpawnerIcon, after: call =>
            {
                var result = call.Result<string>();
                global::KingdomDevSpawner.SpawnedItemIcon.Postfix((WorkspaceObject)call.Subject, ref result);
                call.SetResult(result);
            }));
        }

        private void DisposeHooks()
        {
            foreach (var patch in patches)
                patch.Dispose();
            patches.Clear();
        }

        void OnDestroy()
        {
            DisposeHooks();
            modContext?.Dispose();
        }
    }
}
