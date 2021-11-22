using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UseButton : MonoBehaviour
{
    [System.NonSerialized]
    public Useable target;

    public Button button;
    public Image buttonImage;
    public Image arrowLeft;
    public Image arrowRight;
    public Image arrowTop;
    public Image arrowBottom;

    Coroutine arrowAnimation;

    public void OnPress()
    {
        if(target != null)
        {
            target.PerformAction();
        }
    }

    public void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame || Gamepad.current.xButton.wasPressedThisFrame)
        {
            OnPress();
        }
    }

    public void OnEnable()
    {
        if (arrowAnimation != null)
            StopCoroutine(arrowAnimation);

        arrowAnimation = StartCoroutine(AnimateArrows());
    }

    public void OnDisable()
    {
        if (arrowAnimation != null)
        {
            StopCoroutine(arrowAnimation);
            arrowAnimation = null;
        }
    }

    IEnumerator AnimateArrows()
    {
        arrowLeft.gameObject.SetActive(true);
        arrowRight.gameObject.SetActive(true);
        arrowTop.gameObject.SetActive(true);
        arrowBottom.gameObject.SetActive(true);

        arrowLeft.canvasRenderer.SetAlpha(0);
        arrowRight.canvasRenderer.SetAlpha(0);
        arrowTop.canvasRenderer.SetAlpha(0);
        arrowBottom.canvasRenderer.SetAlpha(0);

        yield return Util.Blend(0.5f, t =>
        {
            var t1 = Curve.InQuad(t);
            arrowLeft.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(-400, -120, t1), 0);
            arrowRight.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(400, 120, t1), 0);
            arrowTop.rectTransform.anchoredPosition = new Vector2(0, Mathf.Lerp(400, 120, t1));
            arrowBottom.rectTransform.anchoredPosition = new Vector2(0, Mathf.Lerp(-400, -120, t1));

            var t2 = Curve.OutQuadInv(t);
            arrowLeft.canvasRenderer.SetAlpha(t2);
            arrowRight.canvasRenderer.SetAlpha(t2);
            arrowTop.canvasRenderer.SetAlpha(t2);
            arrowBottom.canvasRenderer.SetAlpha(t2);
        });

        for (int i = 0; i != 3; ++i)
        {
            yield return Util.Blend(0.5f, t =>
            {
                t = Curve.OutQuadInv(t);
                arrowLeft.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(-120, -180, t), 0);
                arrowRight.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(120, 180, t), 0);
                arrowTop.rectTransform.anchoredPosition = new Vector2(0, Mathf.Lerp(120, 180, t));
                arrowBottom.rectTransform.anchoredPosition = new Vector2(0, Mathf.Lerp(-120, -180, t));
            });

            yield return Util.Blend(1.0f, t =>
            {
                t = Curve.BounceIn(t);
                arrowLeft.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(-180, -120, t), 0);
                arrowRight.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(180, 120, t), 0);
                arrowTop.rectTransform.anchoredPosition = new Vector2(0, Mathf.Lerp(180, 120, t));
                arrowBottom.rectTransform.anchoredPosition = new Vector2(0, Mathf.Lerp(-180, -120, t));
            });
        }

        yield return Util.Blend(0.5f, t =>
        {
            t = Curve.InCubeInv(t);
            arrowLeft.canvasRenderer.SetAlpha(t);
            arrowRight.canvasRenderer.SetAlpha(t);
            arrowTop.canvasRenderer.SetAlpha(t);
            arrowBottom.canvasRenderer.SetAlpha(t);
        });

        arrowLeft.gameObject.SetActive(false);
        arrowRight.gameObject.SetActive(false);
        arrowTop.gameObject.SetActive(false);
        arrowBottom.gameObject.SetActive(false);

        arrowAnimation = null;
    }
}
