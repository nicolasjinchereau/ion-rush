using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortCircuit : MonoBehaviour
{
    public ParticleSystem flash;
    public ParticleSystem sparks;

    public void Play(bool destroyOnStop = false)
    {
        if (destroyOnStop) {
            var flashMain = flash.main;
            var sparksMain = sparks.main;
            flashMain.stopAction = ParticleSystemStopAction.Destroy;
            sparksMain.stopAction = ParticleSystemStopAction.Destroy;
        }

        flash.Play();
        sparks.Play();
    }
}
