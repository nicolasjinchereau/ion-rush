using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GraphicExtensions
{
    public static void SetToTextureSize(this Image img)
    {
        var tex = img.sprite.texture;
        img.rectTransform.sizeDelta = new Vector2(tex.width, tex.height);
    }
}
