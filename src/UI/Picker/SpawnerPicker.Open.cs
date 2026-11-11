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
        internal void Open()
        {
            search.SetTextWithoutNotify(""); Filter("");
            GameUI.Instance.TogglePopupMenu(this, true);
            transform.SetAsLastSibling(); openedFrame = Time.frameCount;
            restoreInput = GameInput.Enabled; GameInput.Disable();
            Fit();
            search.Select(); search.ActivateInputField();
        }
    }
}
