using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SmallGearSpawn : MonoBehaviour
{
    public AnimationClip gearAnimation;
    public GameObject smallGearPrefab;
    public GameObject smallGoldGearPrefab;

    public int preSpawnGearCount = 0;
    public float gearSpawnDelay = 1.0f;
    public bool playImpactSound = true;

    public bool spawnOneGoldGear = false;
    public System.Action goldGearCallback;

    public List<SmallGear> spawnedGears = new List<SmallGear>();

    float nextGearSpawn = 0;

    private void Start()
    {
        for (int i = 0; i != preSpawnGearCount; ++i)
        {
            var go = Instantiate(spawnOneGoldGear ? smallGoldGearPrefab : smallGearPrefab, transform.position, transform.rotation);
            
            var smallGear = go.GetComponent<SmallGear>();
            smallGear.gearSpawn = this;
            spawnedGears.Add(smallGear);
            smallGear.playImpactSound = playImpactSound;

            var anim = go.GetComponent<Animation>();
            anim.AddClip(gearAnimation, "GearAnimation");
            anim.Play("GearAnimation");
            anim["GearAnimation"].time = gearSpawnDelay * (preSpawnGearCount - i);
            spawnOneGoldGear = false;
        }
    }

    void Update()
    {
        if(Time.time >= nextGearSpawn)
        {
            var go = Instantiate(spawnOneGoldGear ? smallGoldGearPrefab : smallGearPrefab, transform.position, transform.rotation);
            
            var smallGear = go.GetComponent<SmallGear>();
            smallGear.gearSpawn = this;
            spawnedGears.Add(smallGear);
            smallGear.playImpactSound = playImpactSound;

            var anim = go.GetComponent<Animation>();
            anim.AddClip(gearAnimation, "GearAnimation");
            anim.Play("GearAnimation");

            if (spawnOneGoldGear) {
                smallGear.callback = goldGearCallback;
                goldGearCallback = null;
            }

            spawnOneGoldGear = false;
            nextGearSpawn = Time.time + gearSpawnDelay;
        }
    }

    public void DestroySpawnedGears()
    {
        foreach (var smallGear in spawnedGears)
        {
            if (smallGear)
            {
                smallGear.gearSpawn = null; // avoid recursion
                Destroy(smallGear.gameObject);
            }
        }

        spawnedGears.Clear();
    }
}
