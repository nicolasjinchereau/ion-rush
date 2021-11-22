using UnityEngine;
using System.Collections;

public class SmallGear : MonoBehaviour
{
    public Animation anim;
    public bool isGold = false;
    public bool playImpactSound = true;
    public System.Action callback;
    public SmallGearSpawn gearSpawn;

    public void PlayGearClank()
    {
        if(playImpactSound)
            SharedSounds.gearClank.Play();
    }

    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (gearSpawn) {
            gearSpawn.spawnedGears.Remove(this);
        }
    }

    public void OnGearInfrontOfPlayer()
    {

    }

    public void OnCallback()
    {
        if (callback != null)
        {
            callback();
            callback = null;
        }
    }
}
