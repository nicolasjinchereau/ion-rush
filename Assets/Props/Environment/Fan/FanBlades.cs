using UnityEngine;
using System.Collections;

public class FanBlades : MonoBehaviour 
{
    public float rpm = 1.0f;
    
    void Update()
    {
        transform.localRotation *= Quaternion.Euler(0, Time.deltaTime * -rpm * 360.0f, 0);
    }
}
