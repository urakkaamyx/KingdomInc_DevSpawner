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
        Button Button(Transform parent, string text, float x, float y, float width, float height, Action action)
        {
            var image = Box(parent, text + "Button", x, y, width, height, buttonSprite);
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            if (text.Length > 0) Text(image.transform, text, 0, 0, width, height, 18).alignment = TextAlignmentOptions.Center;
            button.onClick.AddListener(() => action()); return button;
        }
    }
}
