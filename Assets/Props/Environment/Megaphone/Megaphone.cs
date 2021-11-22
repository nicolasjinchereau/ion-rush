using UnityEngine;
using System.Collections;

public class Megaphone : MonoBehaviour
{
    Coroutine buzzRoutine = null;

    public Animation anim;
    public AudioSource soundSource;

    public bool bypassEffects
    {
        get { return soundSource.bypassEffects; }
        set { soundSource.bypassEffects = value; }
    }

    public void Buzz(AudioClip clip = null)
    {
        Stop();
        buzzRoutine = StartCoroutine(buzz());

        if(clip != null)
            soundSource.clip = clip;

        soundSource.Play();
    }

    public void Stop()
    {
        if(buzzRoutine != null)
        {
            StopCoroutine(buzzRoutine);
            buzzRoutine = null;
        }

        soundSource.Stop();
    }

    IEnumerator buzz()
    {
        var length = soundSource.clip.length;
        var finish = Time.time + length;

        anim["play"].speed = 10.0f;

        while(Time.time < finish)
        {
            anim.Rewind("play");
            anim.Play("play");
            yield return new WaitForSeconds(0.1f);
        }

        buzzRoutine = null;
    }
}
