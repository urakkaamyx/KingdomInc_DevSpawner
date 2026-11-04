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
                Check(player.HasTech("dev_item_spawner"), "Spawner technology gate enabled");
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
                check(emptyDisplay.Caption == "Connect an item input", "Unset node displays connection hint");
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
