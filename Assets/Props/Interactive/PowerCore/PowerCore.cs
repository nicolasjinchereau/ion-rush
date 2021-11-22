using UnityEngine;
using System.Collections;

public class PowerCore : MonoBehaviour
{
    public Transform topPiece;
    public Light lightSource;
    public LightningBolt[] lightningBolts;
    public AudioSource chargeSound;
    public float maxVolume = 0.8f;
    
    public bool IsRunning { get; private set; }
    public bool IsAnimating { get; private set; }
    public bool isCharging { get; private set; }
    
    const float accelleration = 420.0f;
    const float maxRotationSpeed = 700.0f;
    
    Color lightningColor = Color.white;
    float rotationSpeed = 0.0f;
    
    void Update()
    {
        if(IsRunning)
        {
            if(rotationSpeed < maxRotationSpeed)
                rotationSpeed = Mathf.Min(rotationSpeed + Time.deltaTime * accelleration, maxRotationSpeed);
        }
        else
        {
            if(rotationSpeed > 0.0f)
                rotationSpeed = Mathf.Max(rotationSpeed - Time.deltaTime * accelleration, 0.0f);
        }
        
        if(rotationSpeed > 0.0001f)
        {
            if(!IsAnimating)
            {
                IsAnimating = true;
                
                lightningBolts[0].enabled = true;
                lightningBolts[1].enabled = true;
                lightningBolts[2].enabled = true;
                lightSource.intensity = 0;
                lightSource.enabled = true;
                
                chargeSound.pitch = 0;
                chargeSound.volume = 0;
                chargeSound.Play();
            }
            
            isCharging = rotationSpeed > maxRotationSpeed * 0.95f;
            
            float f = rotationSpeed / maxRotationSpeed;
            chargeSound.pitch = f;
            chargeSound.volume = f * maxVolume;
            
            float flicker = 1.0f - Mathf.Clamp01(Mathf.PerlinNoise(Time.time * 20.0f, 0.5f) * 3.0f);
            lightSource.intensity = f * flicker * 5.0f;
            
            lightningColor.a = f * (0.9f * flicker + 0.1f);
            lightningBolts[0].meshRenderer.material.SetColor("_TintColor", lightningColor);
            lightningBolts[1].meshRenderer.material.SetColor("_TintColor", lightningColor);
            lightningBolts[2].meshRenderer.material.SetColor("_TintColor", lightningColor);
            
            topPiece.transform.rotation = Quaternion.Euler(0, Time.deltaTime * (rotationSpeed + 20), 0) * topPiece.transform.rotation;
        }
        else
        {
            if(IsAnimating)
            {
                IsAnimating = false;
                lightSource.enabled = false;
                lightningBolts[0].enabled = false;
                lightningBolts[1].enabled = false;
                lightningBolts[2].enabled = false;
                
                if(chargeSound.isPlaying)
                    chargeSound.Stop();
                
                isCharging = false;
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
            IsRunning = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
            IsRunning = false;
    }
}
