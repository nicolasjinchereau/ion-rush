using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBlocker : MonoBehaviour
{
    public Collider col;

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player" && Player.position.x > transform.position.x)
        {
            col.isTrigger = false;
        }
    }
}
