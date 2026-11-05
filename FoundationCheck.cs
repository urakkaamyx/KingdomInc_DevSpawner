#if DEVSPAWNER_DIAGNOSTICS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.TechTree;
using Assets.Source.Flow;
using Assets.Source.Flow.Elements;
using Assets.Source.Items;
using Assets.Source.Player;
using BepInEx;
using HarmonyLib;
using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomDevSpawner
{
    public sealed partial class DevSpawnerPlugin
    {
        void VerifyFoundationModel(GamePlayer player, Action<bool, string> check)
        {
            check(FoundationApi.TechNodes.TryGet(ResearchId, out var registration)
                && registration.IsRegistered && ((INativeTechNode)registration).NativeNode == SpawnerResearch,
                "Foundation owns the live DevSpawner research registration");
            check(!player.HasTech(SpawnerResearch) && !player.HasTech("dev_item_spawner"), "Fresh kingdom starts with Spawner locked");
            var node = new DevSpawner { Item = ItemType.Get("iron_ore") };
            node.Update(0);
            check(node.RequiredTech == SpawnerResearch && node.Outputs[0].BufferedItems.Count == 0
                && !FlowElement.GetBuildableTypes().Any(e => e is DevSpawner), "Research gates both native building and native generation");
            check(!SpawnerResearch.IsLocked && SpawnerResearch.IsAvailable
                && SpawnerResearch.GetCost().GetCurrency().All(c => c.amt == 0), "Foundation unlock has no prerequisites and costs no currency");
            SpawnerResearch.ResetCost();
            check(SpawnerResearch.GetCost().GetCurrency().All(c => c.amt == 0), "Free research cost survives native cost reset");
            VerifyFoundationSaves(check);
            GamePlayer.Current = player;
        }

        void VerifyFoundationSaves(Action<bool, string> check)
        {
            var previous = GamePlayer.Current;
            try
            {
                var legacy = GamePlayer.CreateNewPlayer();
                var empty = legacy.ToJson();
                var emptyLoaded = GamePlayer.FromJson(empty);
                check(!emptyLoaded.HasTech(SpawnerResearch), "Loading kingdom without Spawners leaves free unlock available");
                GamePlayer.Current = legacy;
                var flow = legacy.Flows[0];
                var spawner = new DevSpawner { Item = ItemType.Get("iron_ore"), Position = new Vector2Int(320, 128) };
                var receiver = new Stockpile();
                receiver.Inputs[0].Item = spawner.Item;
                flow.Elements.Add(spawner); flow.Elements.Add(receiver);
                spawner.Outputs[0].BufferedItems.Add(new InventoryItem(spawner.Item));
                var pipe = new FlowPipe(); pipe.Connect(spawner.Outputs[0], receiver.Inputs[0]);
                pipe.Nodes.Add(new Vector2Int(400, 128)); flow.Pipes.Add(pipe);
                string flowBefore = flow.ToJson().ToString();
                var migrated = GamePlayer.FromJson(legacy.ToJson());
                check(migrated.HasTech(SpawnerResearch) && migrated.HasTech("dev_item_spawner"), "Legacy placed Spawner grants new Foundation unlock on load");
                check(migrated.Flows[0].ToJson().ToString() == flowBefore, "Migration preserves node IDs, positions, selected items, buffers and pipes exactly");
                check(migrated.ToJson()["Tech"].AsJsonArray.Where(v => v.AsString != ResearchId).Select(v => v.AsString)
                    .SequenceEqual(legacy.ToJson()["Tech"].AsJsonArray.Select(v => v.AsString)), "Migration preserves all preexisting research flags");
                var saved = migrated.ToJson();
                check(saved["Tech"].AsJsonArray.Count(v => v.AsString == ResearchId) == 1, "Foundation purchase uses one entry in native saved research list");
                var reloaded = GamePlayer.FromJson(saved);
                check(reloaded.HasTech(SpawnerResearch) && reloaded.Flows[0].ToJson().ToString() == flowBefore, "Migrated kingdom reload preserves unlock and entire workshop");
                // A purchased unlock also persists when all Spawners have been removed.
                reloaded.Flows[0].Elements.RemoveAll(e => e is DevSpawner);
                reloaded.Flows[0].Pipes.Clear();
                check(GamePlayer.FromJson(reloaded.ToJson()).HasTech(SpawnerResearch), "Purchased research persists without any placed Spawner");
            }
            finally { GamePlayer.Current = previous; }
        }

        IEnumerator VerifyFoundationUI(Action<bool, string> check)
        {
            var player = GamePlayer.Current;
            check(!player.HasTech(SpawnerResearch), "New played kingdom requires the free unlock");
            GameUI.Instance.ToggleTechMenu(true);
            yield return null;
            var menu = TechTreeMenu.Instance;
            check(!menu.Nodes.Any(n => n.Tech == SpawnerResearch), "DevSpawner research stays out of Base Game tab");
            var tabs = menu.GetComponent<KingdomFoundation.TechTreeTabs>();
            check(tabs != null, "Foundation tabs attach to native tech tree");
            var mods = (Button)AccessTools.Field(typeof(KingdomFoundation.TechTreeTabs), "ModsButton").GetValue(tabs);
            ExecuteEvents.Execute(mods.gameObject, new PointerEventData(EventSystem.current)
                { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
            yield return null;
            var view = menu.Nodes.Single(n => n.Tech == SpawnerResearch);
            check(view.gameObject.activeInHierarchy && view.GetTooltipTitle() == "DevSpawner", "Mods tab contains one native DevSpawner research node");
            var camera = (Camera)AccessTools.Field(typeof(TechTreeMenu), "_camera").GetValue(menu);
            menu.FocusNode(SpawnerResearch);
            yield return new WaitForSecondsRealtime(.4f);
            view = menu.Nodes.Single(n => n.Tech == SpawnerResearch);
            Canvas.ForceUpdateCanvases();
            var click = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(camera,
                    view.transform.TransformPoint(((RectTransform)view.transform).rect.center)) };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(click, hits);
            check(hits.Any(h => h.gameObject.GetComponentInParent<UITechNode>()?.Tech == SpawnerResearch), "DevSpawner research receives actual pointer raycasts");
            var currency = player.ToJson()["Currency"].ToString();
            ExecuteEvents.Execute(view.gameObject, click, ExecuteEvents.pointerClickHandler);
            check(player.HasTech(SpawnerResearch) && player.HasTech("dev_item_spawner")
                && player.ToJson()["Currency"].ToString() == currency, "Native research click unlocks Spawner without spending currency");
            check(FlowElement.GetBuildableTypes().Count(e => e is DevSpawner) == 1, "Unlocked Resource catalog contains exactly one native Spawner");
            check(player.ToJson()["Tech"].AsJsonArray.Any(v => v.AsString == ResearchId), "Native click persists Foundation research purchase");
            GameUI.Instance.ToggleTechMenu(false);
            yield return null;
        }
    }
}
#endif
