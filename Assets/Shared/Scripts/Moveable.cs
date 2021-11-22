using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : MonoBehaviour
{
    public AudioClip slidingSound;
    public float slidingVolume = 1.0f;

    Rigidbody body;
    AudioSource source;
    const float slideDelay = 0.1f;
    float lastSlid = -slideDelay * 2;
    float slide = 0;
    int floorLayer = 0;
    
    //Vector3 startScale;
    
	void Awake()
    {
        //startScale = transform.localScale;
        var init = SharedSounds.that;
		body = GetComponent<Rigidbody>();
        source = gameObject.AddComponent<AudioSource>();
        source.clip = slidingSound;
        source.time = Random.value * slidingSound.length;
        source.spatialBlend = 0;
        source.volume = 0;
        source.loop = true;
        source.playOnAwake = false;
        source.outputAudioMixerGroup = SharedSounds.MixerGroup;
        source.Stop();
        
        floorLayer = LayerMask.NameToLayer("Floor");
	}
    
    private void OnCollisionStay(Collision c)
    {
        if(c.gameObject.layer == floorLayer)
        {
            Vector3 vel = c.relativeVelocity;
            vel.y = 0;

            float speed = vel.magnitude;
            if(speed > 1.0f)
            {
                //transform.localScale = startScale * 2.0f;
                lastSlid = Time.time;
            }
        }
        
        if(Time.time - lastSlid < slideDelay)
			slide = Mathf.Min(slide + Time.deltaTime * 5.0f, 1.0f);
		else
			slide = Mathf.Max(slide - Time.deltaTime * 5.0f, 0.0f);

        if(slide > 0)
        {
            if(!source.isPlaying)
                source.Play();

            source.volume = (slide * slide) * slidingVolume;
            source.pitch = Mathf.Lerp(0.95f, 1.0f, slide);
        }
        else
        {
            if(source.isPlaying)
                source.Stop();
        }
    }
}
