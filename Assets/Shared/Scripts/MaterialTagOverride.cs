using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialTagOverride : MonoBehaviour
{
    public Material mat;
    public string shaderTag = "";
    public string value = "";

    private void Awake()
    {
        mat.SetOverrideTag(shaderTag, value);
    }
}
