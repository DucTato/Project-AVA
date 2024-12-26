using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class Bar : MonoBehaviour
{
    public enum FillDirection
    {
        Right,
        Left,
        Up,
        Down
    }

    [SerializeField]
    FillDirection fillDirection;
    [SerializeField]
    RectTransform fill;
    public void SetValue(float value)
    {
        if (fillDirection == FillDirection.Right)
        {
            fill.anchorMin = new Vector2(0, 0);
            fill.anchorMax = new Vector2(value, 1);
        }
        else if (fillDirection == FillDirection.Left)
        {
            fill.anchorMin = new Vector2(1 - value, 0);
            fill.anchorMax = new Vector2(1, 1);
        }
        else if (fillDirection == FillDirection.Up)
        {
            fill.anchorMin = new Vector2(0, 0);
            fill.anchorMax = new Vector2(1, value);
        }
        else if (fillDirection == FillDirection.Down)
        {
            fill.anchorMin = new Vector2(0, 1 - value);
            fill.anchorMax = new Vector2(1, 1);
        }
    }
    public void SetAlpha(float value)
    {
        foreach (Graphic img in GetComponentsInChildren<Graphic>())
        {
            img.color = new Color (img.color.r, img.color.g, img.color.b, value);
        }
    }
}