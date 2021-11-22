using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class Gear : MonoBehaviour
{
    [Serializable]
    public class GearCollectedEvent : UnityEvent
    {
    }

    public GameObject gearExplosionPrefab;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Material goldMaterial;
    public Mesh goldGearMesh;
    public bool isGold = false;
    public GearCollectedEvent onGearCollected;

    void Awake()
    {
        if (isGold)
        {
            meshFilter.mesh = goldGearMesh;
            meshRenderer.material = goldMaterial;
            meshRenderer.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        }
    }

    void Update() {
        transform.Rotate(0, 180.0f * Time.deltaTime, 0.0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            GameController.state.OnGearsCollected(transform.position);
            Destroy(gameObject);
            Instantiate(gearExplosionPrefab, transform.position, Quaternion.identity);
            if (onGearCollected != null) onGearCollected.Invoke();
        }
    }
}
