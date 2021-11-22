using UnityEngine;
using System.Collections;

public class PlasmaShell : MonoBehaviour
{
    public ParticleSystem particles;
    public ParticleSystem shockwave;
    public AudioSource impactSound;
    [System.NonSerialized] public Vector3 velocity = Vector3.zero;
    [System.NonSerialized] public Vector3 gravity = new Vector3(0, -9.8f, 0);
    bool collided = false;

    float velocityScale = 2.0f;

    void Update()
    {
        if(!collided)
        {
            velocity += gravity * Time.deltaTime * velocityScale;
            transform.position += velocity * Time.deltaTime * velocityScale;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collided)
            return;

        collided = true;
        velocity = Vector3.zero;

        impactSound.Play();
        SharedSounds.explosion.Play(0.3f);

        Vector3 pos = transform.position;
        
        for(int i = 0; i < 100; ++i)
        {
            Vector3 vel = Random.insideUnitSphere * 20.0f;
            
            if(vel.y < 0)
                vel.y = -vel.y;

            particles.Emit(pos, vel, 0.3f, 0.3f, Color.white);
        }

        particles.enableEmission = false;

        shockwave.Emit(1);

        Player.DoExplosiveDamage(transform.position, 30.0f, Difficulty.cannonDamageRadius, BatteryDrainReason.PlasmaShell);
    }
}
