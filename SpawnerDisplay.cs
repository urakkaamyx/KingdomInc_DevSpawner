using Assets.Behaviour.Flow;
using Assets.Source.Flow.Elements;
using Assets.Source.Items;
using Assets.Source.Util;
using Behaviour.UI.Tooltip;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace KingdomDevSpawner
{
    public sealed class SpawnerDisplay : MonoBehaviour, ITooltipTitleSource, ITooltipTextSource
    {
        static readonly System.Reflection.FieldInfo LabelField = AccessTools.Field(typeof(WorldFlowElement), "_label");
        WorldFlowElement world;
        TMP_Text caption;
        ItemType lastItem;
        Translation lastLanguage;
        bool initialized;

        internal string Caption => caption ? caption.text : null;

        void LateUpdate() => Refresh();

        public void Refresh()
        {
            if (!world) world = GetComponent<WorldFlowElement>();
            if (!(world.Element is DevSpawner spawner)) return;
            if (!caption)
            {
                var template = (TMP_Text)LabelField.GetValue(world);
                caption = Instantiate(template, transform, false);
                caption.name = "SpawnerItemName";
                caption.raycastTarget = false;
                caption.richText = false;
                caption.alignment = TextAlignmentOptions.Center;
                caption.fontSize = 14;
                caption.fontSizeMin = 10;
                caption.fontSizeMax = 14;
                caption.enableAutoSizing = true;
                caption.textWrappingMode = TextWrappingModes.NoWrap;
                caption.overflowMode = TextOverflowModes.Ellipsis;
                var rect = caption.rectTransform;
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(.5f, 0);
                rect.offsetMin = new Vector2(12, 6);
                rect.offsetMax = new Vector2(-12, 34);
                rect.localScale = Vector3.one;
                caption.gameObject.SetActive(true);
                var tooltip = gameObject.AddComponent<TooltipSource>();
                tooltip.SetSource(gameObject);
            }
            if (initialized && lastItem == spawner.Item && lastLanguage == Translation.Current) return;
            initialized = true;
            lastItem = spawner.Item;
            lastLanguage = Translation.Current;
            caption.text = lastItem != null ? Translation.Translate(lastItem.GetDisplayName()) : "Connect an item input";
            spawner.Description = GetTooltipText();
            world.UpdateIcon();
        }

        public string GetTooltipTitle() => "Spawner";

        public string GetTooltipText()
        {
            var item = (world?.Element as DevSpawner)?.Item;
            return item != null
                ? "Produces " + Translation.Translate(item.GetDisplayName()) + "."
                : "Connect the output to an input that accepts a specific item to choose what to produce.";
        }
    }
}
