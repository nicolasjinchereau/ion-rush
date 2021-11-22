using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GearButton : MonoBehaviour
{
    public RectTransform rectTransform;
    public Text labelText;

    public void OnTextUpdated()
    {
        var sz = rectTransform.sizeDelta;
        sz.x = labelText.preferredWidth + 120 * 2;
        rectTransform.sizeDelta = sz;
    }
}
