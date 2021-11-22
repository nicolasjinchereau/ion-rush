using UnityEngine;
using System.Collections;

public class WaterRippleController : MonoBehaviour
{
    public ParticleSystem waterRipple;
    private Transform player = null;
    public AudioSource splashSound;

    void Update()
    {
        if(player != null)
        {
            var ppos = player.position;
            var waterSurface = transform.position.y + 0.02f;
            var ripplePos = new Vector3(ppos.x, waterSurface, ppos.z);
            waterRipple.transform.position = ripplePos;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            player = other.transform;
            waterRipple.Play();
            waterRipple.EnableEmission(true);
            splashSound.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            player = null;
            waterRipple.Stop();
        }
    }
}
