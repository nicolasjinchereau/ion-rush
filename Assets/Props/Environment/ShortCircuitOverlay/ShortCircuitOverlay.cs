using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShortCircuitOverlay : MonoBehaviour
{
    public RawImage staticImage;
    public Image vignetteImage;
    public Animation surgeAnimation;

    bool surging = false;

    public void DoShortCircuit() {
        if(!surging)
            StartCoroutine(ShortCircuitRoutine());
    }

    IEnumerator ShortCircuitRoutine()
    {
        surging = true;

        var startColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        var endColorStatic = new Color(1.0f, 1.0f, 1.0f, 0.15f);
        var endColorVignette = new Color(1.0f, 1.0f, 1.0f, 0.7f);

        surgeAnimation.Play("FlashSurges");

        yield return StartCoroutine(Util.Blend(1.0f, t => {
            t = Curve.ArcQuadInSharp(t);
            staticImage.color = Color.Lerp(startColor, endColorStatic, t);
            vignetteImage.color = Color.Lerp(startColor, endColorVignette, t);
        }));

        surging = false;
    }
}
