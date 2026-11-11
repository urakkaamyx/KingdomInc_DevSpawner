using Assets.Behaviour.Flow;
using Assets.Source.Flow.Elements;
using Assets.Source.Items;
using Assets.Source.Util;
using Behaviour.UI.Tooltip;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Behaviour.UI;

namespace KingdomDevSpawner
{
    public sealed partial class SpawnerDisplay : MonoBehaviour, ITooltipTitleSource, ITooltipTextSource
    {
        public string GetTooltipText()
        {
            var item = (world?.Element as DevSpawner)?.Item;
            return item != null
                ? "Produces " + Translation.Translate(item.GetDisplayName()) + ".\n" + SpawnerPicker.ValueText(item) + " at 100% quality, before shop bonuses.\nClick the item icon to choose another item."
                : "Click + to choose an item and see its gold value, or connect the output to a specific item input.";
        }
    }
}
