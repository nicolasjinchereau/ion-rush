using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPanel : MonoBehaviour
{
    public GameObject offScreen;
    public GameObject staticScreen;

    public void SetOn()
    {
        offScreen.SetActive(false);
        staticScreen.SetActive(false);
    }

    public void SetOff()
    {
        offScreen.SetActive(true);
        staticScreen.SetActive(false);
    }

    public void SetStatic()
    {
        offScreen.SetActive(false);
        staticScreen.SetActive(true);
    }
}
