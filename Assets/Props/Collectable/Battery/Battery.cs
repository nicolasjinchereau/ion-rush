using UnityEngine;
using System.Collections;

public class Battery : MonoBehaviour
{
    public GameObject batteryExplosionPrefab;

    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 180.0f * Time.deltaTime, 0.0f) * transform.rotation;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            GameController.state.OnBatteriesCollected(1, transform.position);
            Destroy(gameObject);
            Instantiate(batteryExplosionPrefab, transform.position, Quaternion.identity);
        }
    }
}
