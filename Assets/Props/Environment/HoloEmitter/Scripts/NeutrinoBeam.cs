using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutrinoBeam : MonoBehaviour
{
    public MeshRenderer beam;
    Material mat;
    float startTime;

    void Awake()
    {
        mat = beam.material;
    }

    void Update()
    {
        var offset = mat.mainTextureOffset;
        offset.y = Mathf.Repeat(Time.time * 2.0f, 1.0f);
        mat.mainTextureOffset = offset;
    }
}
