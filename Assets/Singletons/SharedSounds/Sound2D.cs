using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Sound2D : MonoBehaviour
{
    public AudioSource source;
    public bool blocking = false;
    
    float defaultVolume = 1.0f;
    
    private void Awake()
    {
        defaultVolume = source.volume;

        source.playOnAwake = false;
        source.spatialBlend = 0;
        source.bypassEffects = true;
        source.bypassListenerEffects = true;
        source.bypassReverbZones = true;
        source.ignoreListenerVolume = true;
        source.loop = false;
        source.spatialize = false;
    }

    public AudioClip clip {
        get { return source.clip; }
    }

    public void Play() {
        Play(defaultVolume);
    }

    public void Play(float volume)
    {
        if(blocking)
        {
            if(!source.isPlaying)
            {
                source.volume = volume;
                source.Play();
            }
        }
        else
        {
            if (source.isPlaying)
                source.Stop();
            
            source.volume = volume;
            source.Play();
        }
    }

    public void Stop()
    {
        if(source != null)
            source.Stop();
    }
}
