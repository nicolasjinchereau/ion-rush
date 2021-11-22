using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

[Serializable]
public class PressurePlateEvent : UnityEvent
{

}

public class PressurePlate : MonoBehaviour
{
    public GameObject frame;
    public GameObject button;
    public Useable target;
    public PressurePlateEvent onPressureApplied;

    Vector3 startPos;
    int playerLayer;
    Rigidbody body;
    
    void Start()
    {
        startPos = button.transform.position;
        playerLayer = LayerMask.NameToLayer("Player");
        body = button.GetComponent<Rigidbody>();
        Physics.IgnoreCollision(frame.GetComponent<Collider>(), button.GetComponent<Collider>());
    }

    void FixedUpdate()
    {
        if(!playerStanding)
            body.AddForce((startPos - button.transform.position) * 20);

        if(body.position.y > startPos.y)
        {
            body.position = startPos;
            body.velocity = Vector3.zero;
        }
    }

    bool playerStanding = false;

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == playerLayer)
            playerStanding = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.layer == playerLayer)
            playerStanding = false;
    }
}
