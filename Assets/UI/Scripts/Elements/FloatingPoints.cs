using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatingPoints : MonoBehaviour
{
    public RectTransform rectTransform;
    public TMP_Text label;
    public float lifespan = 7.0f;
    public float speed = 100;

    private IEnumerator Start()
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * (speed * lifespan);

        yield return StartCoroutine(Util.Blend(lifespan, t =>
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            label.alpha = Curve.InCubeInv(t);
        }));

        Destroy(gameObject);
    }
}
