using KingdomFoundation.Api;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingdomDevSpawner
{
    internal sealed class Row
    {
        internal Button Button;
        internal Image Icon;
        internal TMP_Text Name, Gold;
        internal FoundationTooltipSource Tooltip;
        internal GameItemType Item;
    }
}
