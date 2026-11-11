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
        void RefreshRows()
        {
            int first = Mathf.Max(0, Mathf.FloorToInt(content.anchoredPosition.y / RowHeight));
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i]; int index = first + i;
                row.Button.gameObject.SetActive(index < matches.Count);
                if (index >= matches.Count) continue;
                row.Item = matches[index];
                ((RectTransform)row.Button.transform).anchoredPosition = new Vector2(0, -index * RowHeight);
                row.Icon.sprite = row.Item.GetIcon();
                row.Name.text = Translation.Translate(row.Item.GetDisplayName());
                row.Gold.text = ValueText(row.Item);
                row.Tooltip.SetTooltip(row.Name.text, row.Gold.text + " at 100% quality, before shop bonuses.\nClick to produce this item. Changing item clears pending Spawner output.");
                row.Button.GetComponent<Image>().color = row.Item == spawner.Item ? new Color(.75f, 1, .65f) : Color.white;
            }
        }
    }
}
