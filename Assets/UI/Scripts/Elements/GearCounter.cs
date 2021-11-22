using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GearCounter : MonoBehaviour
{
    public virtual int gearCount
    {
        get; set;
    }

    public virtual int gearQuota
    {
        get; set;
    }

    public abstract void AddGear(Vector3 fromWorldPosition);
}
