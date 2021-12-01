using UnityEngine;
using System.Collections;

public class UseTriggerBox : UseTrigger 
{
    void Start()
    {
        Destroy(GetComponent<MeshRenderer>());
        Destroy(GetComponent<MeshFilter>());
    }
}
