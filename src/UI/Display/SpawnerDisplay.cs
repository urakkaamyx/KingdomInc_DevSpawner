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
        static readonly System.Reflection.FieldInfo LabelField = AccessTools.Field(typeof(WorldFlowElement), "_label");
        WorldFlowElement world;
        TMP_Text caption;
        ItemType lastItem;
        Translation lastLanguage;
        bool initialized;
        SpawnerPicker picker;

        internal string Caption => caption ? caption.text : null;


        void LateUpdate() => Refresh();
    }
}
