using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : MonoBehaviour
{
    public MeshRenderer eventHorizonRenderer;
    public Material eventHorizonMat;
    public Material accretionDiskMat;
    public Material accretionParticles1Mat;
    public Material gravLensingParticlesMat;
    public float flickerPower = 2.0f;
    public float flickerStrength = 0.5f;
    public Transform accretionDisk;
    public Transform accretionDiskParticles;
    public float particleRotationSpeed = 20.0f;
    public ParticleSystem[] allParticles;
    public GameObject explosionFlash;

    Color flickerColor;
    float flicker = 0;
    float flickerTarget = 0;
    float flickerThreshold = 0.5f;
    float nextFlicker = 0;
    float flickerInterval = 0.05f;
    float particleRotationSpeedScale = 1.0f;
    Coroutine initRoutine = null;

    private void Awake()
    {
        eventHorizonMat = eventHorizonRenderer.material;
        flickerColor = eventHorizonMat.GetColor("_RimColor");
    }

    private void OnEnable()
    {
        if (initRoutine != null)
            StopCoroutine(initRoutine);
        
        initRoutine = StartCoroutine(InitializeParticles());
    }

    IEnumerator InitializeParticles()
    {
        foreach (var particles in allParticles)
        {
            var mainModule = particles.main;
            mainModule.simulationSpeed = 10.0f;
        }

        particleRotationSpeedScale = 10.0f;

        yield return new WaitForSeconds(2.0f);

        foreach (var particles in allParticles)
        {
            var mainModule = particles.main;
            mainModule.simulationSpeed = 0.3f;
        }

        particleRotationSpeedScale = 0.3f;

        initRoutine = null;
    }

    void Update()
    {
        if (Time.time >= nextFlicker)
        {
            flickerTarget = Random.value;
            nextFlicker = Time.time + flickerInterval;
        }

        if (flicker < flickerTarget)
            flicker = Mathf.Min(flicker + Time.deltaTime / flickerInterval, flickerTarget);
        else
            flicker = Mathf.Max(flicker - Time.deltaTime / flickerInterval, flickerTarget);

        float value = Mathf.Pow(flicker, flickerPower) * flickerStrength;

        Color c = flickerColor;
        c.r *= value;
        c.g *= value;
        c.b *= value;
        eventHorizonMat.SetColor("_Color", c);

        accretionDisk.Rotate(Vector3.up, Time.deltaTime * particleRotationSpeed * particleRotationSpeedScale);
        accretionDiskParticles.Rotate(Vector3.up, Time.deltaTime * particleRotationSpeed * particleRotationSpeedScale);
    }
}
