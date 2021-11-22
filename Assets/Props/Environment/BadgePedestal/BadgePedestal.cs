using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadgePedestal : MonoBehaviour
{
    public GameObject badgePivot;
    public float rotationSpeed = 0.1f;
    public float bounceSpeed = 0.5f;
    public float bounceHeight = 0.003f;

    public void Update()
    {
        badgePivot.transform.Rotate(0, Time.deltaTime * 360.0f * rotationSpeed, 0, Space.Self);
        badgePivot.transform.localPosition = new Vector3(0, Mathf.Sin(Time.time * Mathf.PI * 2 * bounceSpeed) * bounceHeight, 0);
    }
}
