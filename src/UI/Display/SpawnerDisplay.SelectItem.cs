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
        internal void SelectItem(ItemType item)
        {
            var spawner = (DevSpawner)world.Element;
            if (spawner.Item != item)
            {
                spawner.Outputs[0].BufferedItems.Clear();
                spawner.Item = item;
            }
            Refresh();
        }
    }
}
