#if DEVSPAWNER_DIAGNOSTICS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.Util;
using Assets.Source.Flow;
using Assets.Source.Flow.Elements;
using Assets.Source.Items;
using Assets.Source.Player;
using Assets.Source.Util;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Assets.Behaviour.UI;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace KingdomDevSpawner
{
    public sealed partial class DevSpawnerPlugin
    {
        static bool Diagnostic => Environment.GetCommandLineArgs().Contains("-devspawner-smoke");
        IEnumerator Verify()
        {
            var checks = new List<string>();
            void Check(bool value, string label) { if (!value) throw new Exception(label); checks.Add(label); }
            float deadline = Time.realtimeSinceStartup + 35;
            while (!FlowPrefabManager.Instance && Time.realtimeSinceStartup < deadline) yield return null;
            bool passed = false;
            string error = null;
            var previous = GamePlayer.Current;
            try
            {
                Check(FlowPrefabManager.Instance != null, "Native prefab manager initialized");
                var player = new GamePlayer();
                GamePlayer.Current = player;
                player.Flows.Add(new FlowDiagram());
                VerifyFoundationModel(player, Check);
                player.AddTech(SpawnerResearch);
                Check(player.HasTech("dev_item_spawner"), "Purchased Foundation research enables native production gate");
                Check(!player.HasTech("forge"), "Unrelated research remains locked");
                Check(!player.HasTech(null), "Null technology behavior preserved");
                Check(TechNode.Get("dev_item_spawner").DevMode, "Developer technology definition preserved");
                Check(FlowPrefabSet.Get("DevSpawner")?.Prefab != null, "Native Spawner world prefab exists");
                Check(FlowElement.GetBuildableTypes().Any(x => x is Assets.Source.Flow.Elements.DevSpawner), "Spawner appears in native build catalog");
                var spawner = new Assets.Source.Flow.Elements.DevSpawner { Item = ItemType.Get("iron_ore") };
                spawner.Update(1);
                Check(spawner.Outputs[0].BufferedItems.Count == 1 && spawner.Outputs[0].BufferedItems[0].Type.Identifier == "iron_ore", "Native Spawner creates selected item");
                spawner.Update(1);
                Check(spawner.Outputs[0].BufferedItems.Count == 1, "Native output buffer prevents overproduction");
                var restored = (Assets.Source.Flow.Elements.DevSpawner)FlowElement.FromJson(spawner.ToJson(), player.Flows[0]);
                Check(restored.Item.Identifier == "iron_ore", "Native save format retains selected item");
                var human = new Assets.Source.Flow.Elements.DevSpawner { Item = ItemType.Get("fighter") };
                human.Update(1);
                Check(human.Outputs[0].BufferedItems.Single().Type.Identifier == "fighter", "Native adventurer spawning works");
                Check(spawner.IconName == spawner.Item.IconName, "Selected item supplies native Spawner icon");
                VerifyDisplay(Check);
                passed = true;
            }
            catch (Exception ex) { error = ex.ToString(); Logger.LogError(ex); }
            finally { GamePlayer.Current = previous; }
            if (passed)
            {
                passed = false;
                try
                {
                    var fixture = GamePlayer.CreateNewPlayer();
                    fixture.Flows[0].Elements.Clear(); fixture.Flows[0].Pipes.Clear();
                    fixture.Flows[0].Elements.Add(new Assets.Source.Flow.Elements.DevSpawner { Position = new Vector2Int(-96, 64) });
                    SceneManager.LoadScene("Game");
                }
                catch (Exception ex) { error = ex.ToString(); Logger.LogError(ex); }
                if (error == null)
                {
                    yield return new WaitForSecondsRealtime(4);
                    var foundationCheck = VerifyFoundationUI(Check);
                    while (error == null)
                    {
                        bool more = false; object step = null;
                        try { more = foundationCheck.MoveNext(); if (more) step = foundationCheck.Current; }
                        catch (Exception ex) { error = ex.ToString(); Logger.LogError(ex); }
                        if (!more || error != null) break;
                        yield return step;
                    }
                    (foundationCheck as IDisposable)?.Dispose();
                    if (error == null)
                    {
                        try { VerifyPicker(Check); passed = true; }
                        catch (Exception ex) { error = ex.ToString(); Logger.LogError(ex); }
                    }
                }
            }
            string folder = Path.Combine(Paths.GameRootPath, "_Mods", "DevSpawner", "Verification");
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "runtime-tests.txt"), "passed=" + passed + "\n" + string.Join("\n", checks) + "\n" + error);
            Application.Quit(passed ? 0 : 1);
        }

        void VerifyDisplay(Action<bool, string> check)
        {
            var root = new GameObject("SpawnerPreview", typeof(Canvas));
            var cameraObject = new GameObject("SpawnerPreviewCamera", typeof(Camera));
            var canvas = root.GetComponent<Canvas>();
            var camera = cameraObject.GetComponent<Camera>();
            var target = RenderTexture.GetTemporary(960, 400, 24);
            var previous = RenderTexture.active;
            var texture = new Texture2D(960, 400, TextureFormat.RGB24, false);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.08f, .08f, .09f);
            camera.cullingMask = 1 << 30;
            camera.orthographic = true;
            camera.orthographicSize = 100;
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 10;
            canvas.scaleFactor = 2;
            try
            {
                var prefab = FlowPrefabSet.Get("DevSpawner").Prefab;
                var empty = Instantiate(prefab, root.transform, false);
                empty.enabled = false;
                var unset = new Assets.Source.Flow.Elements.DevSpawner { Position = new Vector2Int(-208, 64) };
                empty.SetElement(unset);
                var emptyDisplay = empty.GetComponent<SpawnerDisplay>();
                check(emptyDisplay.Caption == "Click + to choose", "Unset node displays clickable picker hint");
                var selected = Instantiate(prefab, root.transform, false);
                selected.enabled = false;
                var model = new Assets.Source.Flow.Elements.DevSpawner { Position = new Vector2Int(16, 64) };
                selected.SetElement(model);
                var receiver = new Stockpile();
                receiver.Inputs[0].Item = ItemType.Get("iron_ore");
                new FlowPipe().Connect(model.Outputs[0], receiver.Inputs[0]);
                model.Update(0);
                var display = selected.GetComponent<SpawnerDisplay>();
                display.Refresh();
                check(model.Item == receiver.Inputs[0].Item && display.Caption == Translation.Translate(model.Item.GetDisplayName()),
                    "Connection selects item and visible caption updates");
                check(display.GetTooltipText().Contains(display.Caption) && model.Description.Contains(display.Caption),
                    "Hover description names produced item");
                var icon = (Image)AccessTools.Field(typeof(WorldFlowElement), "_icon").GetValue(selected);
                check(icon.sprite == model.Item.GetIcon(), "World icon displays produced item");
                selected.UpdateVisuals();
                check(icon.sprite == model.Item.GetIcon() && selected.GetComponents<SpawnerDisplay>().Length == 1,
                    "Native refresh retains item icon and single display");
                var label = selected.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "SpawnerItemName");
                model.Item = ItemType.All.OrderByDescending(i => Translation.Translate(i.GetDisplayName()).Length).First();
                display.Refresh();
                label.ForceMeshUpdate();
                check(label.text == Translation.Translate(model.Item.GetDisplayName()) && label.overflowMode == TextOverflowModes.Ellipsis,
                    "Long names use bounded caption with full hover description");
                model.Item = ItemType.Get("iron_ore"); display.Refresh();
                foreach (var child in root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 30;
                foreach (var child in root.GetComponentsInChildren<RectTransform>(true))
                    if (child.name == "DevSpawner(Clone)") child.anchorMin = child.anchorMax = new Vector2(.5f, .5f);
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 960, 400), 0, 0); texture.Apply();
                var folder = Path.Combine(Paths.GameRootPath, "_Mods", "DevSpawner", "Verification");
                Directory.CreateDirectory(folder);
                File.WriteAllBytes(Path.Combine(folder, "spawner-display.png"), texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                camera.targetTexture = null;
                DestroyImmediate(root); DestroyImmediate(cameraObject); DestroyImmediate(texture);
                RenderTexture.ReleaseTemporary(target);
            }
        }

        void VerifyPicker(Action<bool, string> check)
        {
            var model = (Assets.Source.Flow.Elements.DevSpawner)GamePlayer.Current.Flows[0].Elements.Single();
            check(model.World && GameUI.Instance && EventSystem.current, "Spawner and UI exist in actual game scene");
            var display = model.World.GetComponent<SpawnerDisplay>();
            var choose = model.World.GetComponentsInChildren<Button>(true).Single(b => b.name == "SpawnerChooseItem");
            check(choose.gameObject.activeInHierarchy && choose.interactable && choose.targetGraphic.raycastTarget,
                "Plus icon is an active clickable button");
            var click = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            Canvas.ForceUpdateCanvases();
            var iconRect = (RectTransform)choose.transform;
            var iconCanvas = choose.GetComponentInParent<Canvas>().rootCanvas;
            click.position = RectTransformUtility.WorldToScreenPoint(iconCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : iconCanvas.worldCamera,
                iconRect.TransformPoint(iconRect.rect.center));
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(click, hits);
            check(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == choose, "Actual pointer raycast reaches plus button without overlay obstruction");
            bool inputBefore = GameInput.Enabled;
            ExecuteEvents.Execute(choose.gameObject, click, ExecuteEvents.pointerClickHandler);
            var picker = FindObjectsByType<SpawnerPicker>(FindObjectsSortMode.None).Single();
            check(picker.gameObject.activeInHierarchy && !GameInput.Enabled, "Clicking plus opens picker and blocks workshop hotkeys");
            check(picker.MatchCount == ItemType.All.Count, "Picker includes full cheat item catalog including locked items");
            check(picker.GetComponentsInChildren<Button>(true).Count(b => b.name == "SpawnerItemRow") == 11,
                "Scrollable catalog uses bounded row pool");
            check(ItemType.All.All(item => !string.IsNullOrEmpty(SpawnerPicker.ValueText(item))), "All " + ItemType.All.Count + " catalog entries have a value or non-sellable label");
            var iron = ItemType.Get("iron_ore");
            check(SpawnerPicker.ValueText(iron) == new InventoryItem(iron).SellValue.ToString("N0") + " gold",
                "Gold uses native item sell value at 100% quality");
            check(SpawnerPicker.ValueText(ItemType.Get("fighter")) == "Not sellable", "Non-sellable adventurers are identified correctly");
            var search = picker.GetComponentInChildren<TMP_InputField>();
            search.text = "IRON ORE";
            check(picker.MatchCount > 0 && picker.VisibleItems.Contains(iron), "Typing filters by case-insensitive localized item name");
            search.text = "no_such_item_987654";
            check(picker.MatchCount == 0 && !picker.VisibleItems.Any(), "No-match search clears stale clickable rows");
            search.text = "";
            check(picker.MatchCount == ItemType.All.Count, "Clearing search restores all items");
            var firstPage = picker.VisibleItems.ToArray();
            var scroll = picker.GetComponentInChildren<ScrollRect>();
            scroll.verticalNormalizedPosition = 0; scroll.onValueChanged.Invoke(Vector2.zero);
            check(picker.VisibleItems.Any() && !picker.VisibleItems.SequenceEqual(firstPage)
                && picker.VisibleItems.Contains(ItemType.All.OrderBy(i => Translation.Translate(i.GetDisplayName()), StringComparer.CurrentCultureIgnoreCase).Last()), "Scrolling reaches final catalog item with recycled rows");
            search.text = "iron ore";
            var row = picker.GetComponentsInChildren<Button>().Single(b => b.name == "SpawnerItemRow" && b.GetComponentInChildren<TMP_Text>().text == Translation.Translate(iron.GetDisplayName()));
            ExecuteEvents.Execute(row.gameObject, click, ExecuteEvents.pointerClickHandler);
            check(model.Item == iron && !picker.gameObject.activeSelf, "Clicking item row selects spawning type and closes picker");
            check(GameInput.Enabled == inputBefore && display.Caption == Translation.Translate(iron.GetDisplayName()), "Selection restores workshop input and updates node display");
            model.Update(0);
            check(model.Outputs[0].BufferedItems.Single().Type == iron, "Chosen item is actually generated by native Spawner");
            var loaded = (Assets.Source.Flow.Elements.DevSpawner)FlowElement.FromJson(model.ToJson(), GamePlayer.Current.Flows[0]);
            check(loaded.Item == iron, "Manual selection persists through native save roundtrip");
            display.OpenPicker();
            picker.Filter("iron ingot");
            var ingot = ItemType.Get("iron_ingot");
            var receiver = new Stockpile();
            var connection = new FlowPipe(); connection.Connect(model.Outputs[0], receiver.Inputs[0]);
            picker.Choose(ingot);
            check(model.Item == ingot && model.Outputs[0].BufferedItems.Count == 0 && model.Outputs[0].ConnectedPipe == connection,
                "Changing selection clears old pending output and preserves pipe connection");
            model.Update(0);
            check(model.Outputs[0].BufferedItems.Single().Type == ingot, "Changed selection produces new item without old buffer blockage");
            display.SelectItem(ingot);
            check(model.Outputs[0].BufferedItems.Count == 1, "Selecting same item preserves pending output");
            display.OpenPicker();
            picker.GetComponentsInChildren<Button>().Single(b => b.name == "XButton").onClick.Invoke();
            check(model.Item == ingot && !picker.gameObject.activeSelf && GameInput.Enabled == inputBefore, "Close button cancels without changing item or input state");
            display.OpenPicker();
            GameUI.Instance.ProcessEscape();
            check(!picker.gameObject.activeSelf && model.Item == ingot, "Native Escape handling dismisses picker without selection");
            display.OpenPicker();
            search.text = "iron";
            CapturePicker(picker);
            picker.ClosePopup();
        }

        void CapturePicker(SpawnerPicker picker)
        {
            var canvas = picker.GetComponent<Canvas>();
            var oldParent = picker.transform.parent;
            var oldMode = canvas.renderMode;
            var oldCamera = canvas.worldCamera;
            float oldDistance = canvas.planeDistance, oldScale = canvas.scaleFactor;
            var layers = picker.GetComponentsInChildren<Transform>(true).Select(t => (t, t.gameObject.layer)).ToArray();
            var cameraObject = new GameObject("SpawnerPickerCapture", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>();
            var target = RenderTexture.GetTemporary(900, 700, 24);
            var previous = RenderTexture.active;
            var texture = new Texture2D(900, 700, TextureFormat.RGB24, false);
            try
            {
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.08f, .08f, .09f);
                camera.orthographic = true; camera.orthographicSize = 350; camera.cullingMask = 1 << 31;
                camera.targetTexture = target;
                foreach (var entry in layers) entry.t.gameObject.layer = 31;
                picker.transform.SetParent(null, false);
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 10; canvas.scaleFactor = 1;
                Canvas.ForceUpdateCanvases(); camera.Render();
                RenderTexture.active = target; texture.ReadPixels(new Rect(0, 0, 900, 700), 0, 0); texture.Apply();
                var folder = Path.Combine(Paths.GameRootPath, "_Mods", "DevSpawner", "Verification");
                Directory.CreateDirectory(folder); File.WriteAllBytes(Path.Combine(folder, "spawner-item-picker.png"), texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous; camera.targetTexture = null;
                canvas.renderMode = oldMode; canvas.worldCamera = oldCamera; canvas.planeDistance = oldDistance; canvas.scaleFactor = oldScale;
                picker.transform.SetParent(oldParent, false);
                foreach (var entry in layers) if (entry.t) entry.t.gameObject.layer = entry.layer;
                DestroyImmediate(cameraObject); DestroyImmediate(texture); RenderTexture.ReleaseTemporary(target);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(SaveGame), nameof(SaveGame.Store))]
        static class BlockSaves { static bool Prefix() => !Diagnostic; }
        [HarmonyLib.HarmonyPatch(typeof(SaveGame), nameof(SaveGame.StoreAutosaveState))]
        static class BlockAutosaves { static bool Prefix() => !Diagnostic; }
        [HarmonyLib.HarmonyPatch(typeof(SaveGame), nameof(SaveGame.LoadLatestSave))]
        static class BlockLoads { static bool Prefix(ref bool __result) { if (!Diagnostic) return true; __result = false; return false; } }
        [HarmonyLib.HarmonyPatch(typeof(SteamAchievement), nameof(SteamAchievement.Trigger))]
        static class BlockAchievements { static bool Prefix() => !Diagnostic; }
        [HarmonyLib.HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Initialized), HarmonyLib.MethodType.Getter)]
        static class Offline { static bool Prefix(ref bool __result) { if (!Diagnostic) return true; __result = false; return false; } }
    }
}
#endif
