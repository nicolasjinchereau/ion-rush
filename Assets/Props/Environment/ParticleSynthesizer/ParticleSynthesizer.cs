using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleSynthesizer : MonoBehaviour
{
    public Transform innerSpinner;
    public Transform outerSpinner;
    public Transform spool01;
    public Transform spool02;
    public Transform spool03;
    public Transform spool04;

    public MeshRenderer particleContainerRenderer;
    public Transform particleContainer;
    public Transform containerRestingPosition;
    public Transform containerSynthesisPosition;
    public Transform containerReadyPosition;

    public GameObject resultParticles;
    public GameObject backgroundHaze;
    public ParticleSystem particleSynthesisParticles;
    public List<GameObject> synthesisBeams;

    public AudioSource machineHum;
    public AudioSource electricHum;

    private bool didStartSynthesis = false;
    private Material particleContainerMaterial;
    private Vector3 spoolStartPos01;
    private Vector3 spoolStartPos02;
    private Vector3 spoolStartPos03;
    private Vector3 spoolStartPos04;
    private float spoolSlideOffset = 0.0f;
    private float conductorSpin = 0.0f;

    void Start()
    {
        spoolStartPos01 = spool01.localPosition;
        spoolStartPos02 = spool02.localPosition;
        spoolStartPos03 = spool03.localPosition;
        spoolStartPos04 = spool04.localPosition;
        particleContainerMaterial = particleContainerRenderer.material;
        particleContainerMaterial.SetColor("_Color", new Color(1, 1, 1, 0));
        particleContainer.position = containerRestingPosition.position;

        StartCoroutine(DoParticleSynthesis());
    }

    public void ResetDevice()
    {
        StopAllCoroutines();

        didStartSynthesis = false;
        conductorSpin = 0;
        spool01.localPosition = spoolStartPos01;
        spool02.localPosition = spoolStartPos02;
        spool03.localPosition = spoolStartPos03;
        spool04.localPosition = spoolStartPos04;
        particleContainerMaterial.SetColor("_Color", new Color(1, 1, 1, 0));
        particleContainer.position = containerRestingPosition.position;

        synthesisBeams.ForEach(beam => beam.SetActive(false));
        particleSynthesisParticles.SetActive(false);
        resultParticles.SetActive(false);
        backgroundHaze.SetActive(false);
    }

    IEnumerator DoParticleSynthesis()
    {
        Vector3 synthesisParticlesMinScale = new Vector3(0.02f, 0.02f, 0.02f);
        Vector3 synthesisParticlesMaxScale = new Vector3(0.06f, 0.06f, 0.06f);
        bool synthesisComplete = false;
        
        float spinUpDuration = 4.0f;
        float synthesisDuration = 9.0f;

        machineHum.pitch = 0;
        machineHum.volume = 0;
        machineHum.Play();

        electricHum.volume = 0;
        electricHum.Play();

        StartCoroutine(Util.InvokeDelayed(spinUpDuration + 0.5f, () =>
        {
            synthesisBeams.ForEach(beam => beam.SetActive(true));
            particleSynthesisParticles.SetActive(true);
            particleSynthesisParticles.transform.localScale = synthesisParticlesMinScale;
            StartCoroutine(Util.Blend(0.1f, t => electricHum.volume = t));

            StartCoroutine(Util.Blend(synthesisDuration, t =>
            {
                t = Util.NormalizedClamp(t, 0.0f, 0.8f);

                particleSynthesisParticles.transform.localScale = Vector3.Lerp(
                    synthesisParticlesMinScale,
                    synthesisParticlesMaxScale,
                    t);

            }, () => synthesisComplete = true));
        }));

        yield return StartCoroutine(Util.Blend(spinUpDuration, t => {
            conductorSpin = t;
            
            var containerHeight = Curve.SmoothStepIn(Util.NormalizedClamp(t, 0.0f, 0.75f));

            particleContainer.position = Vector3.Lerp(
                containerRestingPosition.position,
                containerSynthesisPosition.position,
                containerHeight);
        }));

        yield return new WaitForSeconds(0.4f);
        yield return StartCoroutine(Util.Blend(0.1f, t => spoolSlideOffset = t));

        // synthesizing indefinitely...
    }

    public void StopSynthesis()
    {
        StopAllCoroutines();
        StartCoroutine(StopSynthesisRoutine());
    }

    IEnumerator StopSynthesisRoutine()
    {
        StartCoroutine(Util.Blend(0.1f, t => electricHum.volume = Curve.OutQuad(t)));
        synthesisBeams.ForEach(beam => beam.SetActive(false));
        particleSynthesisParticles.SetActive(false);
        resultParticles.SetActive(true);
        backgroundHaze.SetActive(true);

        yield return StartCoroutine(Util.Blend(5.0f, t =>
        {
            conductorSpin = Curve.OutQuad(t);
            spoolSlideOffset = Curve.SmoothStepOut(t);

            particleContainerMaterial.SetColor("_Color", new Color(1, 1, 1, Curve.InQuadInv(t)));
        }));

        machineHum.Stop();
        electricHum.Stop();
    }

    void Update()
    {
    // spin the conductors
        conductorSpin = Mathf.Clamp01(conductorSpin);
        float actualConductorSpin = Mathf.Lerp(30.0f, 450.0f, conductorSpin);
        innerSpinner.localRotation = Quaternion.Euler(0, Time.deltaTime * actualConductorSpin, 0) * innerSpinner.localRotation;
        outerSpinner.localRotation = Quaternion.Euler(0, Time.deltaTime * -actualConductorSpin, 0) * outerSpinner.localRotation;

        var maxHumVolume = 0.5f;
        machineHum.pitch = Curve.OutQuadInv(conductorSpin);
        machineHum.volume = Curve.OutQuadInv(conductorSpin) * maxHumVolume;

        // offset spools
        spoolSlideOffset = Mathf.Clamp01(spoolSlideOffset);
        float actualSpoolOffset = Mathf.Lerp(-0.03f, 0.05f, spoolSlideOffset);
        spool01.localPosition = spoolStartPos01 + spool01.localRotation * Vector3.up * actualSpoolOffset;
        spool02.localPosition = spoolStartPos02 + spool02.localRotation * Vector3.up * actualSpoolOffset;
        spool03.localPosition = spoolStartPos03 + spool03.localRotation * Vector3.up * actualSpoolOffset;
        spool04.localPosition = spoolStartPos04 + spool04.localRotation * Vector3.up * actualSpoolOffset;
    }
}
