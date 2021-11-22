using UnityEngine;
using System.Collections;

public class ForwardTrigger : MonoBehaviour
{
    public GameObject target;
    
    void OnTriggerEnter(Collider other)
    {
        target.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
    }

    void OnTriggerStay(Collider other)
    {
        target.SendMessage("OnTriggerStay", other, SendMessageOptions.DontRequireReceiver);
    }

    void OnTriggerExit(Collider other)
    {
        target.SendMessage("OnTriggerExit", other, SendMessageOptions.DontRequireReceiver);
    }

    void OnCollisionEnter(Collision collision)
    {
        target.SendMessage("OnCollisionEnter", collision, SendMessageOptions.DontRequireReceiver);
    }

    void OnCollisionStay(Collision collision)
    {
        target.SendMessage("OnCollisionStay", collision, SendMessageOptions.DontRequireReceiver);
    }

    void OnCollisionExit(Collision collision)
    {
        target.SendMessage("OnCollisionExit", collision, SendMessageOptions.DontRequireReceiver);
    }
}
