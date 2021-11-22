using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ParticleSystemExtensions
{
    public static void EnableEmission(this ParticleSystem particles, bool enable)
    {
        var emission = particles.emission;
        emission.enabled = enable;

        if(enable && !particles.isPlaying)
            particles.Play();
        else if(!enable && particles.isPlaying)
            particles.Stop();
    }

    public static void Emit(this ParticleSystem particles)
    {
        var rateOverTime = particles.emission.rateOverTime;

        if(rateOverTime.mode != ParticleSystemCurveMode.TwoConstants)
            throw new System.InvalidOperationException("rateOverTime.mode must be ParticleSystemCurveMode.TwoConstants");

        var count = Random.Range(rateOverTime.constantMin, rateOverTime.constantMax);
        particles.Emit(Mathf.RoundToInt(count));
    }
}
