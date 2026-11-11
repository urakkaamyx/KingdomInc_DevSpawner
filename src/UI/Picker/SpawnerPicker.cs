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
        const float Width = 660, Height = 570, RowHeight = 44;
        const int PoolSize = 11;
        SpawnerDisplay owner;
        DevSpawner spawner;
        TMP_FontAsset font;
        RectTransform panel, content;
        TMP_InputField search;
        TMP_Text count;
        ScrollRect scroll;
        readonly List<ItemType> matches = new List<ItemType>();
        readonly List<Row> rows = new List<Row>();
        ItemType[] catalog;
        bool restoreInput;
        int openedFrame;
        Sprite panelSprite, buttonSprite;
        internal int MatchCount => matches.Count;
        internal IEnumerable<ItemType> VisibleItems => rows.Where(r => r.Button.gameObject.activeSelf).Select(r => r.Item);


        internal static string ValueText(ItemType item)
        {
            var sample = new InventoryItem(item);
            if (!Shop.CanSellItem(sample)) return "Not sellable";
            try { return sample.SellValue.ToString("N0", CultureInfo.CurrentCulture) + " gold"; }
            catch (NullReferenceException) { return "Value unavailable"; }
        }
    }
}
