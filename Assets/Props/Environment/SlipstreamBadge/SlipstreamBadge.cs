using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlipstreamBadge : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public Texture stationCubemap;
    public Texture spaceCubemap;
    public ReflectionProbe reflectionProbe;
    public Color offlineColor = new Color(0, 0, 0, 0);
    public Color onlineColor = new Color32(176, 151, 0, 255);

    public Material Material {
        get { return mat; }
    }

    Material mat;
    bool _online = false;

    private void Awake()
    {
        mat = new Material(meshRenderer.sharedMaterial);
        mat.SetColor("_EmissiveColor", Color.clear);
        meshRenderer.material = mat;
    }

    public void UseStationReflection() {
        reflectionProbe.SetActive(false);
        mat.SetTexture("_Cube", stationCubemap);
    }

    public void UseSpaceReflection() {
        reflectionProbe.SetActive(false);
        mat.SetTexture("_Cube", spaceCubemap);
    }

    public void UseRealtimeReflection()
    {
        reflectionProbe.SetActive(true);
        mat.SetTexture("_Cube", reflectionProbe.realtimeTexture);
    }

    private void Update()
    {
        if(reflectionProbe.gameObject.activeSelf)
            mat.SetTexture("_Cube", reflectionProbe.realtimeTexture);
    }

    public bool Online
    {
        set
        {
            _online = value;
            
            if (_online) {
                mat.SetColor("_EmissiveColor", onlineColor);
            }
            else {
                mat.SetColor("_EmissiveColor", offlineColor);
            }
        }
        get
        {
            return _online;
        }
    }
}
