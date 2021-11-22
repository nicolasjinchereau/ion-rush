using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ComponentExtensions
{
    /// <summary>
    /// Equivalent to Component.gameObject.SetActive(value).
    /// </summary>
    public static void SetActive(this Component component, bool value) {
        component.gameObject.SetActive(value);
    }

    public static void UsePreferredWidth(this RectTransform rectTransform)
    {
        var sz = rectTransform.sizeDelta;
        var width = LayoutUtility.GetPreferredWidth(rectTransform);
        rectTransform.sizeDelta = new Vector2(width, sz.y);
    }

    public static void UsePreferredHeight(this RectTransform rectTransform)
    {
        var sz = rectTransform.sizeDelta;
        var height = LayoutUtility.GetPreferredHeight(rectTransform);
        rectTransform.sizeDelta = new Vector2(sz.x, height);
    }

    public static void UsePreferredSize(this RectTransform rectTransform)
    {
        var width = LayoutUtility.GetPreferredWidth(rectTransform);
        var height = LayoutUtility.GetPreferredHeight(rectTransform);
        rectTransform.sizeDelta = new Vector2(width, height);
    }
}
