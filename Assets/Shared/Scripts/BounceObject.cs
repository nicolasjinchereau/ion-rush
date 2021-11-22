using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceObject : MonoBehaviour
{
    public Vector3 bounce = new Vector3(0.5f, 0, 0);
    public float speed = 1.0f;
    Vector3 startPos;

    private void Awake()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float t = -Mathf.Cos(Time.time * Mathf.PI * 2.0f * speed) * 0.5f + 0.5f;
        transform.position = Vector3.Lerp(startPos, startPos + bounce, t);
    }
}
