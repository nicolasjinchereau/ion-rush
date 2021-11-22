using UnityEngine;
using System.Collections;
using System;

public class LevelEnd : MonoBehaviour
{
    public BigDoors bigDoors;
    [NonSerialized] public bool playerDidExit = false;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            playerDidExit = true;
        }
    }
}
