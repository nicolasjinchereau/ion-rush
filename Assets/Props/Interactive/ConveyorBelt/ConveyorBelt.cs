using UnityEngine;
using System.Collections;

public class ConveyorBelt : MonoBehaviour
{
    public float textureScrollSpeed = 1.0f;
    public float wheelSpeed = 1.0f;
    public bool forceActive = false;

    public Vector3 direction = Vector3.zero;
    public float beltLength = 0;
    public GameObject conveyorSurface;
    private Material conveyorMaterial;
    Vector2 textureOffset = Vector2.zero;

    public Transform[] wheels;
    public ConveyorPath belt;

    public bool glitchy = false;
    public float glitchTimeOffset = 0.0f;

    public ParticleSystem[] shortCircuits;

    bool isGlitching = false;
    float startingBeltRPM = 0.0f;
    float nextGlitchUpdate = 0;

    public float glitchSlowTime = 1.0f;
    public float glitchFastTime = 3.0f;
    public float glitchFastRPMFactor = 5.0f;
    public float glitchSlowRPMFactor = 0.25f;

    int playerLayer = 0;

    void Start()
    {
        BoxCollider box = conveyorSurface.GetComponent<BoxCollider>();
        
        float diameter = box.size.y;
        float radius = diameter * 0.5f;

        float sideLength = Mathf.PI * diameter * 0.5f;
        float topLength = box.size.x - (radius * 2.0f);

        beltLength = topLength * 2.0f + sideLength * 2.0f;

        conveyorMaterial = conveyorSurface.GetComponent<Renderer>().material;

        playerLayer = LayerMask.NameToLayer("Player");
        belt.enabled = forceActive;

        if (glitchy)
        {
            nextGlitchUpdate = Time.time + glitchTimeOffset + glitchSlowTime;
            startingBeltRPM = belt.conveyorRPM;

            foreach(var ss in shortCircuits) {
                ss.SetActive(true);
            }
        }
    }

    void OnEnable()
    {
        if(forceActive)
            belt.enabled = true;
    }

    void OnDisable()
    {
        if(forceActive)
            belt.enabled = false;
    }

    void Update()
    {
        textureOffset.x += textureScrollSpeed * belt.conveyorRPM * (1.0f / beltLength) * Time.deltaTime;
        conveyorMaterial.mainTextureOffset = textureOffset;

        foreach(Transform wheel in wheels)
        {
            wheel.localRotation = Quaternion.Euler(0, 0, Time.deltaTime * wheelSpeed * belt.conveyorRPM * 360.0f) * wheel.localRotation;
        }

        if (glitchy)
        {
            if (Time.time >= nextGlitchUpdate)
            {
                if (!isGlitching)
                {
                    isGlitching = true;
                    belt.conveyorRPM = startingBeltRPM * glitchFastRPMFactor;
                    nextGlitchUpdate = Time.time + glitchFastTime;

                    foreach (var ss in shortCircuits) {
                        ss.Play();
                    }
                }
                else
                {
                    isGlitching = false;
                    belt.conveyorRPM = startingBeltRPM * glitchSlowRPMFactor;
                    nextGlitchUpdate = Time.time + glitchSlowTime;
                }
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if(!forceActive && collider.gameObject.layer == playerLayer)
            belt.enabled = true;
    }

    void OnTriggerExit(Collider collider)
    {
        if(!forceActive && collider.gameObject.layer == playerLayer)
            belt.enabled = false;
    }
}
