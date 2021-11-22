using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    public Transform arm1;
    public Transform arm2;
    public Transform claw1;
    public Transform claw2; // neg
    
    public bool isExtended = false;

    [Range(0.0f, 1.0f)]
    public float closeAmount = 0;

    Vector3 arm1RetractedLocalPos = new Vector3(0, 0, 0.28f);
    Vector3 arm1ExtendedLocalPos = new Vector3(0, 0, 0);
    Vector3 arm2RetractedLocalPos = new Vector3(0, 0.234f, 0);
    Vector3 arm2ExtendedLocalPos = new Vector3(0, 0.624f, 0);

    float extensionTime = 1.0f;
    float extensionAmount = 0;

    void Update()
    {
        if (isExtended)
        {
            extensionAmount = Mathf.Min(extensionAmount + Time.deltaTime / extensionTime, 1.0f);
        }
        else
        {
            extensionAmount = Mathf.Max(extensionAmount - Time.deltaTime / extensionTime, 0);
        }

        arm1.localPosition = Vector3.Lerp(arm1RetractedLocalPos, arm1ExtendedLocalPos, extensionAmount);
        arm2.localPosition = Vector3.Lerp(arm2RetractedLocalPos, arm2ExtendedLocalPos, extensionAmount);

        claw1.localRotation = Quaternion.Euler(0, 0, closeAmount * 45.0f);
        claw2.localRotation = Quaternion.Euler(0, 0, closeAmount * -45.0f);
    }
}
