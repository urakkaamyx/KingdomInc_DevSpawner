using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomDevSpawner
{
    public sealed class SpawnerDisplay : MonoBehaviour, IFoundationTooltipTitleSource, IFoundationTooltipTextSource
    {
        UnityEngine.Behaviour world;
        TMP_Text caption;
        GameItemType lastItem;
        string lastLanguage;
        bool initialized;
        SpawnerPicker picker;
        internal string Caption => caption ? caption.text : null;

        void LateUpdate() => Refresh();
        public string GetTooltipText()
        {
            var item = FoundationApi.WorkspaceAccess.FromView(world)?.SpawnedItem;
            return item != null ? "Produces " + FoundationApi.Presentation.Translate(item.GetDisplayName()) + ".\n" + SpawnerPicker.ValueText(item) + " at 100% quality, before shop bonuses.\nClick the item icon to choose another item." : "Click + to choose an item and see its gold value, or connect the output to a specific item input.";
        }

        public string GetTooltipTitle() => "Spawner";
        void OnDestroy()
        {
            if (picker)
            {
                if (picker.gameObject.activeSelf && FoundationApi.UiAccess.Root)
                    picker.ClosePopup();
                Destroy(picker.gameObject);
            }
        }

        void OnDisable()
        {
            if (picker && picker.gameObject.activeSelf && FoundationApi.UiAccess.Root)
                picker.ClosePopup();
        }

        public void OpenPicker()
        {
            var spawner = FoundationApi.WorkspaceAccess.FromView(world);
            if (!FoundationApi.UiAccess.Root || spawner == null || !spawner.IsSpawner)
                return;
            if (!picker)
            {
                var root = new GameObject("SpawnerItemPicker", typeof(RectTransform));
                root.SetActive(false);
                picker = root.AddComponent<SpawnerPicker>();
                picker.Initialize(this, spawner, caption.font, FoundationApi.UiAccess.ItemFilterTemplate.transform.parent);
            }

            picker.Open();
        }

        public void Refresh()
        {
            if (!world)
                world = FoundationApi.WorkspaceAccess.FindBuildingView(gameObject);
            var spawner = FoundationApi.WorkspaceAccess.FromView(world);
            if (spawner == null || !spawner.IsSpawner)
                return;
            if (!caption)
            {
                var template = FoundationApi.WorkspaceVisuals.LabelText(world);
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
                var tooltip = gameObject.AddComponent<FoundationTooltipSource>();
                tooltip.SetSource(gameObject);
                var icon = FoundationApi.WorkspaceVisuals.Icon(world);
                icon.raycastTarget = true;
                var choose = icon.GetComponent<Button>() ?? icon.gameObject.AddComponent<Button>();
                choose.targetGraphic = icon;
                choose.navigation = new Navigation
                {
                    mode = Navigation.Mode.None
                };
                choose.onClick = new Button.ButtonClickedEvent();
                choose.onClick.AddListener(OpenPicker);
                choose.name = "SpawnerChooseItem";
            }

            if (initialized && lastItem == spawner.SpawnedItem && lastLanguage == FoundationApi.Presentation.CurrentLocale)
                return;
            initialized = true;
            lastItem = spawner.SpawnedItem;
            lastLanguage = FoundationApi.Presentation.CurrentLocale;
            caption.text = lastItem != null ? FoundationApi.Presentation.Translate(lastItem.GetDisplayName()) : "Click + to choose";
            spawner.Description = GetTooltipText();
            FoundationApi.WorkspaceVisuals.RefreshIcon(world);
        }

        internal void SelectItem(GameItemType item)
        {
            var spawner = FoundationApi.WorkspaceAccess.FromView(world);
            if (spawner.SpawnedItem != item)
            {
                spawner.Outputs[0].ClearBuffer();
                spawner.SpawnedItem = item;
            }

            Refresh();
        }
    }
}
