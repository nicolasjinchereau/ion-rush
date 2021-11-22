using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBomb : MonoBehaviour
{
    public GameObject detonator;
    public MeshRenderer containerRenderer;
    public MeshRenderer detonatorLights;
    public Material lightsOffMaterial;
    public Material lightsOnMaterial;
    public AudioSource clinkSound;

    bool armed = false;

    public bool IsArmed
    {
        get { return armed; }
        set
        {
            armed = value;
            detonatorLights.material = armed ? lightsOnMaterial : lightsOffMaterial;
        }
    }
}
