using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoloEmitter : MonoBehaviour
{
    public Image starImage;
    public Image anomalyImage;
    public Image noiseImage;
    public Transform clipIndicator;
    public MeshRenderer clipIndicatorRenderer;
    public MeshRenderer projectionRenderer;
    public ParticleSystem emissionParticles;
    public Transform emitterBase;
    public Transform projectionPlane;

    public GameObject neutrinoEmitter;
    public MeshRenderer neutrinoEmitterRenderer;
    public GameObject neutrinoBeam;
    public ParticleSystem neutrinoReaction;
    public Transform neutrinoSurgePos1;
    public Transform neutrinoSurgePos2;
    public Transform neutrinoFeedback;
    public ParticleSystem neutrinoEmitterSparks;

    public AudioClip activateSound;
    public AudioClip deactivateSound;

    Vector2 starPos = new Vector2(-30, 0);
    Vector2 starSize = new Vector2(160, 160);
    float starAlpha = 0.2078431f;

    Vector2 anomalyPos = new Vector2(70, 0);
    Vector2 anomalySize = new Vector2(20, 20);
    float anomalyAlpha1 = 0.1647059f;
    float anomalyAlpha2 = 0.5803922f;

    Vector2 blackHolePos = new Vector2(25.4f, 0);
    Vector2 blackHoleSize = new Vector2(50, 50);
    float blackHoleAlpha = 1.0f;

    float noiseAlpha = 0.01176471f;

    float projectorClipStart = 0.495f; // 0.499f

    Vector3 projectionPlanePos = new Vector3(0, 0.001f, 0);
    Vector3 projectionPlaneScale = new Vector3(1.2f, 1.2f, 0.6f);

    Material mat;
    Color capColorStart;
    Color capLineColorStart;
    Color clipIndicatorColor;

    Color neutrinoEmitterColorA;
    Color neutrinoEmitterColorB;
    Color projectionMainColor;
    Color projectionLineColor;
    Color projectionCapMainColor;
    Color projectionCapLineColor;
    Color emissionLineColor;

    private void Awake()
    {
        mat = projectionRenderer.material;
        capColorStart = mat.GetColor("_CapColor");
        capLineColorStart = mat.GetColor("_CapLineColor");
        clipIndicatorColor = clipIndicatorRenderer.material.color;
        neutrinoEmitterColorA = neutrinoEmitterRenderer.material.GetColor("_ColorA");
        neutrinoEmitterColorB = neutrinoEmitterRenderer.material.GetColor("_ColorB");
        projectionMainColor = mat.GetColor("_Color");
        projectionLineColor = mat.GetColor("_LineColor");
        projectionCapMainColor = mat.GetColor("_CapColor");
        projectionCapLineColor = mat.GetColor("_CapLineColor");
        emissionLineColor = emissionParticles.main.startColor.color;
        ResetSimulation();
    }

    private IEnumerator Start()
    {
        while (true)
        {
            yield return StartCoroutine(StartEmitterRoutine());
            yield return StartCoroutine(RunSimulationRoutine());
            yield return StartCoroutine(StopEmitterRoutine());
            yield return new WaitForSeconds(1.0f);
        }
    }
    
    void ResetSimulation()
    {
        starImage.rectTransform.anchoredPosition = starPos;
        starImage.rectTransform.sizeDelta = starSize;
        starImage.color = new Color(1, 1, 1, starAlpha);

        anomalyImage.rectTransform.anchoredPosition = anomalyPos;
        anomalyImage.rectTransform.sizeDelta = anomalySize;
        anomalyImage.color = new Color(1, 1, 1, 0);

        noiseImage.color = new Color(1, 1, 1, noiseAlpha);
        mat.SetFloat("_Height", 0);
        mat.SetFloat("_Clip", projectorClipStart);

        clipIndicator.SetActive(false);
        clipIndicatorRenderer.material.color = clipIndicatorColor;

        mat.SetColor("_CapColor", capColorStart);
        mat.SetColor("_CapLineColor", capLineColorStart);

        neutrinoBeam.SetActive(false);
        neutrinoEmitter.SetActive(false);
        neutrinoEmitterRenderer.material.SetColor("_ColorA", neutrinoEmitterColorA);
        neutrinoEmitterRenderer.material.SetColor("_ColorB", neutrinoEmitterColorB);
        neutrinoReaction.SetActive(false);
        neutrinoFeedback.SetActive(false);
        neutrinoFeedback.position = neutrinoSurgePos1.position;
        neutrinoEmitterSparks.Stop();
        neutrinoEmitterSparks.SetActive(false);

        mat.SetColor("_Color", Color.clear);
        mat.SetColor("_LineColor", Color.clear);
        mat.SetColor("_CapColor", Color.clear);
        mat.SetColor("_CapLineColor", Color.clear);

        emissionParticles.Stop();
        var main = emissionParticles.main;
        var startColor = main.startColor;
        startColor.color = Color.clear;
        main.startColor = startColor;

        projectionPlane.localPosition = projectionPlanePos;
        projectionPlane.localScale = projectionPlaneScale;
    }

    IEnumerator StartEmitterRoutine()
    {
        emissionParticles.Play();

        var startPos = emitterBase.transform.position;
        var endPos = projectionPlane.position;
        var startScale = Vector3.zero;
        var endScale = projectionPlane.localScale;

        // fade in color of hologram
        yield return StartCoroutine(Util.Blend(0.5f, t =>
        {
            t = Curve.OutPowerInv(t, 7);
            mat.SetColor("_Color", Color.Lerp(Color.clear, projectionMainColor, t));
            mat.SetColor("_LineColor", Color.Lerp(Color.clear, projectionLineColor, t));
            mat.SetColor("_CapColor", Color.Lerp(Color.clear, projectionCapMainColor, t));
            mat.SetColor("_CapLineColor", Color.Lerp(Color.clear, projectionCapLineColor, t));

            var main = emissionParticles.main;
            var startColor = main.startColor;
            startColor.color = Color.Lerp(Color.clear, emissionLineColor, t);
            main.startColor = startColor;

            projectionPlane.position = Vector3.Lerp(startPos, endPos, t);
            projectionPlane.localScale = Vector3.Lerp(startScale, endScale, t);
        }));
    }

    IEnumerator StopEmitterRoutine()
    {
        emissionParticles.Stop();

        clipIndicator.SetActive(false);

        var capCurrentColor = mat.GetColor("_CapColor");
        var capLineCurrentColor = mat.GetColor("_CapLineColor");

        var startPos = projectionPlane.position;
        var endPos = emitterBase.transform.position;
        var startScale = projectionPlane.localScale;
        var endScale = Vector3.zero;

        // fade in color of hologram
        yield return StartCoroutine(Util.Blend(0.3f, t =>
        {
            t = Curve.InQuad(t);
            mat.SetColor("_Color", Color.Lerp(projectionMainColor, Color.clear, t));
            mat.SetColor("_LineColor", Color.Lerp(projectionLineColor, Color.clear, t));
            mat.SetColor("_CapColor", Color.Lerp(capCurrentColor, Color.clear, t));
            mat.SetColor("_CapLineColor", Color.Lerp(capLineCurrentColor, Color.clear, t));

            var main = emissionParticles.main;
            var startColor = main.startColor;
            startColor.color = Color.Lerp(emissionLineColor, Color.clear, t);
            main.startColor = startColor;

            projectionPlane.position = Vector3.Lerp(startPos, endPos, t);
            projectionPlane.localScale = Vector3.Lerp(startScale, endScale, t);
        }));

        ResetSimulation();
    }

    IEnumerator RunSimulationRoutine()
    {
        yield return new WaitForSeconds(1.5f);

// SHOW STAR
        // fade height from 0 to 1 to show star
        yield return StartCoroutine(Util.Blend(1.0f, t => {
            mat.SetFloat("_Height", Curve.OutQuadInv(t));
        }));

        yield return new WaitForSeconds(3.0f);

// SHOW ANOMALY APPEAR
        float jitterStrength = 0;

        // fade in anomaly
        StartCoroutine(Util.Blend(3.0f, t => {
            anomalyImage.color = new Color(1, 1, 1, Mathf.Lerp(0, anomalyAlpha1, Curve.SmoothStepInSteep(t)));
            jitterStrength = t;
        }));

        // jitter the x/z scale of the star
        yield return StartCoroutine(Util.InvokeRepeating(5.0f, time =>
        {
            var jit = Mathf.PerlinNoise(time * 5.0f, 0.5f);
            var jit2 = Mathf.PerlinNoise((time + 10) * 5.0f, 0.5f);

            var sz = starSize;
            sz.x *= Mathf.Lerp(0.9f, 1.1f, jit);
            sz.y *= Mathf.Lerp(0.9f, 1.1f, Mathf.Lerp(jit, 1.0f - jit, jit2));

            starImage.rectTransform.sizeDelta = Vector2.Lerp(starSize, sz, jitterStrength);
        }));

// SHOW COLLAPSE
        // fade start back to 1-1-1 scale
        var starJitteredSize = starImage.rectTransform.sizeDelta;
        yield return StartCoroutine(Util.Blend(0.5f, t => {
            starImage.rectTransform.sizeDelta = Vector2.Lerp(starJitteredSize, starSize, Curve.OutQuadInv(t));
        }));

        // show neutrino beam hitting anomaly while it scales down slightly
        neutrinoBeam.SetActive(true);
        neutrinoEmitter.SetActive(true);
        neutrinoReaction.SetActive(true);

        var anomalyDissipatingAlpha = anomalyAlpha1 * 0.3f;

        yield return StartCoroutine(Util.Blend(4.0f, t => {
            anomalyImage.color = new Color(1, 1, 1, Mathf.Lerp(anomalyAlpha1, anomalyDissipatingAlpha, t));
        }));

        // show feedback surge hit emitter
        neutrinoFeedback.SetActive(true);

        yield return StartCoroutine(Util.Blend(1.0f, t => {
            neutrinoFeedback.transform.position = Vector3.Lerp(neutrinoSurgePos1.position, neutrinoSurgePos2.position, t);
        }));

        neutrinoBeam.SetActive(false);
        neutrinoEmitterRenderer.material.SetColor("_ColorA", neutrinoEmitterColorA * 0.75f);
        neutrinoEmitterRenderer.material.SetColor("_ColorB", neutrinoEmitterColorB * 0.75f);
        neutrinoReaction.SetActive(false);
        neutrinoFeedback.SetActive(false);
        neutrinoEmitterSparks.SetActive(true);
        neutrinoEmitterSparks.Play();
        
        // anomaly doubles in size
        yield return StartCoroutine(Util.Blend(4.0f, t => {
            t = Curve.InQuad(t);
            anomalyImage.color = new Color(1, 1, 1, Mathf.Lerp(anomalyDissipatingAlpha, anomalyAlpha2, Curve.InQuad(t)));
        }));

        // show anomaly consume star
        yield return StartCoroutine(Util.Blend(5.0f, t => {
            starImage.rectTransform.anchoredPosition = Vector2.Lerp(starPos, blackHolePos, Curve.RoughStepIn(t));
            anomalyImage.rectTransform.anchoredPosition = Vector2.Lerp(anomalyPos, blackHolePos, Curve.RoughStepIn(t));

            starImage.rectTransform.sizeDelta = Vector2.Lerp(starSize, blackHoleSize, Curve.InPower(t, 2));
            anomalyImage.rectTransform.sizeDelta = Vector2.Lerp(anomalySize, blackHoleSize, Curve.RoughStepIn(t));

            starImage.color = new Color(1, 1, 1, Mathf.Lerp(starAlpha, starAlpha * 0.6f, Curve.InPower(t, 7)));
            anomalyImage.color = new Color(1, 1, 1, Mathf.Lerp(anomalyAlpha2, blackHoleAlpha, Curve.InPower(t, 7)));

            noiseImage.color = new Color(1, 1, 1, Mathf.Lerp(noiseAlpha, 0, Curve.InPower(t, 7)));
        }));

        yield return new WaitForSeconds(2.0f);

// SHOW CLIPPED DATA
        // show clipping indicator and fade in red clipped data
        clipIndicator.SetActive(true);
        var clipMat = clipIndicatorRenderer.material;
        var clipMatDimCol = clipIndicatorColor;
        clipMatDimCol.a = 0;

        var clipIndicatorScale = clipIndicator.localScale;

        yield return StartCoroutine(Util.Blend(1.0f, t => {
            clipIndicator.localScale = Vector3.Lerp(clipIndicatorScale * 2.5f, clipIndicatorScale, Curve.OutQuadInv(t));
            clipMat.color = Color.Lerp(clipMatDimCol, clipIndicatorColor, t);
        }));

        var clipPulseRoutine = StartCoroutine(Util.InvokeRepeating(time => {
            clipIndicator.localScale = Vector3.Lerp(clipIndicatorScale, clipIndicatorScale * 1.1f, Curve.ArcLinear(Mathf.Repeat(time * 1.0f, 1.0f)));
        }));

        yield return new WaitForSeconds(3.0f);

        StopCoroutine(clipPulseRoutine);
        clipPulseRoutine = null;
        
        yield return StartCoroutine(Util.Blend(1.0f, t => {
            clipMat.color = Color.Lerp(clipIndicatorColor, clipMatDimCol, t);
        }));
        clipIndicator.SetActive(false);
        
        yield return new WaitForSeconds(2.0f);

        yield return StartCoroutine(Util.Blend(0.3f, t => {
            mat.SetFloat("_Clip", Mathf.Lerp(projectorClipStart, 1.0f, t));
        }));

        yield return new WaitForSeconds(0.2f);

        var capColor1 = capColorStart;
        var capColor2 = capColor1;
        capColor2 *= 0.1f;

        var capLineColor1 = capLineColorStart;
        var capLineColor2 = capLineColor1;
        capLineColor2 *= 0.1f;

        yield return StartCoroutine(Util.InvokeRepeating(6.0f, time =>
        {
            var t = Mathf.Repeat(time * 1.0f, 1.0f);
            t = Curve.ArcLinear(t);
            var a = Mathf.Lerp(0.1f, 0.3f, t);
            mat.SetColor("_CapColor", Color.Lerp(capColor1, capColor2, t));
            mat.SetColor("_CapLineColor", Color.Lerp(capLineColor1, capLineColor2, t));
        }));

        yield return StartCoroutine(Util.Blend(0.2f, t => {
            mat.SetFloat("_Height", Curve.InQuadInv(t));
        }));
    }
}
