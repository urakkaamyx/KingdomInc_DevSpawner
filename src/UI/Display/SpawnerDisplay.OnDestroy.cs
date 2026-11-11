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
        void OnDestroy() { if (picker) { if (picker.gameObject.activeSelf && GameUI.Instance) picker.ClosePopup(); Destroy(picker.gameObject); } }
    }
}
