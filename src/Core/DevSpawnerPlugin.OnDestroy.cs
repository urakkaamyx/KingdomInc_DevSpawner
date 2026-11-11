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
        void OnDestroy() { patches?.UnpatchSelf(); modContext?.Dispose(); }
    }
}
