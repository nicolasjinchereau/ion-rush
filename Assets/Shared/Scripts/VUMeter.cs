using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VUMeter : MonoBehaviour
{
    [ColorUsage(true, true)]  public Color color;
    public MeshRenderer[] targetMeshes;

    float _level = 0;
    public float Level
    {
        get { return _level; }
        set
        {
            _level = value;

            if (targetMeshes != null)
            {
                foreach (var mesh in targetMeshes)
                {
                    var col = color;
                    col.r *= _level;
                    col.g *= _level;
                    col.b *= _level;
                    mesh.material.SetColor("_EmissiveColor", col);
                }
            }
        }
    }

    void Awake()
    {
        if (targetMeshes != null)
        {
            foreach (var mesh in targetMeshes)
            {
                mesh.material.SetColor("_EmissiveColor", Color.clear);
            }
        }
    }
}
