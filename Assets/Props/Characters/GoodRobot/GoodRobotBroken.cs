using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoodRobotBroken : MonoBehaviour
{
    float wheelCircumference = 0.8206593f;

    public Transform leftFrontWheel;
    public Transform leftBackWheel;
    public Transform rightFrontWheel;
    public Transform rightBackWheel;

    float nextSparkSound = 0.0f;

    public AudioSource movingSound;
    public AudioSource shortCircuitSound;
    public ParticleSystem sparks;

    void Start()
    {
        movingSound.ignoreListenerVolume = true;
    }

    void Update()
    {
        var p = Mathf.PerlinNoise(Time.time, 0.5f);
        p = Curve.OutQuadInv(p);
        float rotationSpeed = Mathf.Lerp(0.0f, 700.0f, p);
        transform.rotation = Quaternion.Euler(0, rotationSpeed * Time.deltaTime, 0) * transform.rotation;

        movingSound.volume = Mathf.Lerp(0.1f, 0.4f, p);
        movingSound.pitch = Mathf.Lerp(0.3f, 2.4f, p);

        if (Time.time >= nextSparkSound)
        {
            shortCircuitSound.Play();
            sparks.Play();
            nextSparkSound = Time.time + Random.Range(0.5f, 2.0f);
        }
    }
}
