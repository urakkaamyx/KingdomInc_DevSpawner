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
        internal void Filter(string query)
        {
            var words = (query ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            matches.Clear();
            foreach (var item in catalog)
            {
                string name = Translation.Translate(item.GetDisplayName());
                if (words.All(w => name.IndexOf(w, StringComparison.CurrentCultureIgnoreCase) >= 0 || item.Identifier.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0)) matches.Add(item);
            }
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(-14, Mathf.Max(352, matches.Count * RowHeight));
            scroll.StopMovement(); scroll.verticalNormalizedPosition = 1;
            count.text = matches.Count == 0 ? "No matching items" : matches.Count + " items - click a row to select";
            RefreshRows();
        }
    }
}
