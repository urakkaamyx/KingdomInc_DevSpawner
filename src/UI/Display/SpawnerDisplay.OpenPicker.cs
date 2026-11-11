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
        public void OpenPicker()
        {
            if (!GameUI.Instance || !(world.Element is DevSpawner spawner)) return;
            if (!picker)
            {
                var root = new GameObject("SpawnerItemPicker", typeof(RectTransform));
                root.SetActive(false);
                picker = root.AddComponent<SpawnerPicker>();
                picker.Initialize(this, spawner, caption.font, GameUI.Instance.FilterPopup.transform.parent);
            }
            picker.Open();
        }
    }
}
