using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public enum CanvasGroupAutoFade
{
    None,
    FadeIn,
    FadeOut,
}

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    public float fadeLength = 0.5f;
    //public bool interactibleDuringFade = false;
    public CanvasGroupAutoFade autoFade = CanvasGroupAutoFade.None;

    void Awake()
    {
        if (!canvasGroup)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (autoFade == CanvasGroupAutoFade.FadeIn)
        {
            canvasGroup.alpha = 0.0f;
            StartCoroutine(DoFadeIn(fadeLength, null));
        }
        else if (autoFade == CanvasGroupAutoFade.FadeOut)
        {
            canvasGroup.alpha = 1.0f;
            StartCoroutine(DoFadeIn(fadeLength, null));
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void FadeIn(float length, Action then)
    {
        StopAllCoroutines();
        StartCoroutine(DoFadeIn(length, then));
    }

    public void FadeOut(float length, Action then)
    {
        StopAllCoroutines();
        StartCoroutine(DoFadeOut(length, then));
    }

    IEnumerator DoFadeIn(float length, Action then)
    {
        float a = canvasGroup.alpha;

        do
        {
            a += Time.deltaTime / length;
            canvasGroup.alpha = Mathf.Min(a, 1);
            yield return null;
        } while (a < 1);

        then?.Invoke();
    }

    IEnumerator DoFadeOut(float length, Action then)
    {
        float a = canvasGroup.alpha;

        do
        {
            a -= Time.deltaTime / length;
            canvasGroup.alpha = Mathf.Max(a, 0);
            yield return null;
        } while (a > 0);

        then?.Invoke();
    }
}
