using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveCollider : MonoBehaviour
{
    public AudioSource impactSound;
    public float radius = 0.2f;
    public float amount = 1.0f;

    void OnCollisionEnter(Collision collision)
    {
        if (!impactSound.isPlaying)
            impactSound.Play();

        foreach (var contact in collision.contacts)
        {
            var body = contact.otherCollider.attachedRigidbody;
            if(body && !body.isKinematic)
            {
                body.AddForceAtPosition(-contact.normal * amount, contact.point);
            }
        }
    }
}
