using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretFire : MonoBehaviour
{
    public ParticleSystem turretBulletDeath;
    private ParticleSystem me;
    private AudioSource impactSound;
    List<ParticleCollisionEvent> collisions = new List<ParticleCollisionEvent>();
    
    void Awake()
    {
        me = GetComponent<ParticleSystem>();
        impactSound = turretBulletDeath.GetComponent<AudioSource>();
        impactSound.ignoreListenerVolume = true;
    }

    void OnParticleCollision(GameObject other)
    {
        collisions.Clear();
        int count = me.GetCollisionEvents(other, collisions);

        for(int i = 0; i < count; ++i)
        {
            turretBulletDeath.transform.position = collisions[i].intersection;
            turretBulletDeath.Emit(5);
            impactSound.Play();
        }

        //Vector3 otherPos = other.transform.position;

        //var particles = new ParticleSystem.Particle[me.particleCount];
        //int particleCount = me.GetParticles(particles);

        //var closestParticle = particles[0];

        //float closest = (closestParticle.position - otherPos).sqrMagnitude;

        //for(int i = 1; i < me.particleCount; ++i)
        //{
        //    float dist = (particles[i].position - otherPos).sqrMagnitude;
        //    if(dist < closest)
        //    {
        //        closestParticle = particles[i];
        //        closest = dist;
        //    }
        //}

        //turretBulletDeath.transform.position = closestParticle.position;
        //turretBulletDeath.Emit(5);
        //impactSound.Play();
    }
}
