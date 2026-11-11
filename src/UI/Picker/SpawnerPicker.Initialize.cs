using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.Shared;
using Assets.Source.Flow.Elements;
using Assets.Source.Items;
using Assets.Source.Util;
using Behaviour.UI.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace KingdomDevSpawner
{
    public sealed partial class SpawnerPicker : UIPopupMenu
    {
        internal void Initialize(SpawnerDisplay display, DevSpawner model, TMP_FontAsset nativeFont, Transform parent)
        {
            owner = display; spawner = model; font = nativeFont;
            catalog = ItemType.All.OrderBy(i => Translation.Translate(i.GetDisplayName()), StringComparer.CurrentCultureIgnoreCase).ToArray();
            var sprites = Resources.FindObjectsOfTypeAll<Sprite>();
            panelSprite = sprites.FirstOrDefault(s => s.name == "Tooltip");
            buttonSprite = sprites.FirstOrDefault(s => s.name == "Button03");
            transform.SetParent(parent, false);
            var root = (RectTransform)transform;
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            var canvas = gameObject.AddComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 100;
            gameObject.AddComponent<GraphicRaycaster>();
            var backdrop = Rect(root, "DismissBackdrop", 0, 0, 0, 0);
            backdrop.anchorMin = Vector2.zero; backdrop.anchorMax = Vector2.one;
            backdrop.offsetMin = backdrop.offsetMax = Vector2.zero;
            backdrop.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, .65f);
            backdrop.gameObject.AddComponent<Button>().onClick.AddListener(ClosePopup);
            panel = Box(root, "ChooseSpawnerItem", 0, 0, Width, Height, panelSprite).rectTransform;
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(.5f, .5f);
            panel.anchoredPosition = Vector2.zero;
            Text(panel, "Choose Spawner item", 24, 16, 550, 34, 24);
            Button(panel, "X", 606, 17, 30, 30, ClosePopup);
            Text(panel, "Gold value at 100% quality, before shop bonuses", 24, 53, 612, 26, 15);

            var searchBox = Box(panel, "SearchItems", 24, 88, 612, 36, buttonSprite);
            search = searchBox.gameObject.AddComponent<TMP_InputField>();
            var textArea = Rect(searchBox.transform, "TextArea", 10, 2, 592, 32);
            textArea.gameObject.AddComponent<RectMask2D>();
            var inputText = Text(textArea, "", 0, 0, 592, 32, 18);
            inputText.richText = false;
            var placeholder = Text(textArea, "Search items...", 0, 0, 592, 32, 18);
            placeholder.color = new Color(.7f, .68f, .6f);
            search.textViewport = textArea; search.textComponent = inputText; search.placeholder = placeholder;
            search.fontAsset = font; search.pointSize = 18; search.lineType = TMP_InputField.LineType.SingleLine;
            search.characterLimit = 100;
            search.onValueChanged.AddListener(Filter);
            Text(panel, "ITEM", 34, 135, 380, 24, 14);
            var goldHeading = Text(panel, "GOLD VALUE", 444, 135, 176, 24, 14);
            goldHeading.alignment = TextAlignmentOptions.MidlineRight;
            var viewport = Rect(panel, "ItemsViewport", 24, 165, 612, 352);
            viewport.gameObject.AddComponent<Image>().color = new Color(.08f, .06f, .05f, .8f);
            viewport.gameObject.AddComponent<RectMask2D>();
            content = Rect(viewport, "ItemRows", 0, 0, 598, 352);
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.sizeDelta = new Vector2(-14, 352);
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 35;
            var track = Box(viewport, "ScrollTrack", 600, 0, 12, 352, null);
            track.color = new Color(.16f, .13f, .1f);
            var handle = Box(track.transform, "Handle", 0, 0, 12, 352, buttonSprite);
            handle.rectTransform.anchorMin = Vector2.zero; handle.rectTransform.anchorMax = Vector2.one;
            handle.rectTransform.offsetMin = handle.rectTransform.offsetMax = Vector2.zero;
            var scrollbar = track.gameObject.AddComponent<Scrollbar>();
            scrollbar.handleRect = handle.rectTransform; scrollbar.targetGraphic = handle;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = scrollbar;
            scroll.onValueChanged.AddListener(_ => RefreshRows());
            for (int i = 0; i < PoolSize; i++)
            {
                var row = new Row();
                row.Button = Button(content, "", 0, i * RowHeight, 598, 42, () => Choose(row.Item));
                row.Button.name = "SpawnerItemRow";
                row.Icon = Rect(row.Button.transform, "ItemIcon", 10, 5, 32, 32).gameObject.AddComponent<Image>();
                row.Icon.preserveAspect = true; row.Icon.raycastTarget = false;
                row.Name = Text(row.Button.transform, "", 52, 0, 342, 42, 17);
                row.Name.overflowMode = TextOverflowModes.Ellipsis;
                row.Gold = Text(row.Button.transform, "", 398, 0, 186, 42, 16);
                row.Gold.alignment = TextAlignmentOptions.MidlineRight;
                row.Gold.enableAutoSizing = true; row.Gold.fontSizeMin = 11; row.Gold.fontSizeMax = 16;
                row.Gold.overflowMode = TextOverflowModes.Ellipsis;
                row.Gold.color = new Color(1, .82f, .35f);
                row.Tooltip = row.Button.gameObject.AddComponent<TooltipSource>();
                rows.Add(row);
            }
            count = Text(panel, "", 24, 527, 612, 24, 14);
            Filter("");
        }
    }
}
