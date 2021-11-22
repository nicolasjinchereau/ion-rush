using UnityEngine;
using System.Collections;

public class PressurePlateTrigger : MonoBehaviour
{
    public PressurePlate plate;

    float nextContact = 0;
    float contactDelay = 0.5f;
    bool inContact = false;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == plate.button)// && Time.time > nextContact)
        {
            nextContact = Time.time + contactDelay;
            inContact = true;

            if (plate.target != null)
            {
                plate.target.OnUseStart();
                plate.target.OnAction();
                
                if(plate.target.unused)
                    plate.target.unused = false;
            }

            plate.onPressureApplied.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject == plate.button && inContact) 
        {
            if(plate.target != null)
            {
                plate.target.OnUseFinish();
            }

            inContact = false;
        }
    }
}
