using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    public int value = 1000;
    public Transform orbitCoins;
    public GameObject explosionPrefab;
    public GameObject triggerGizmo;
    private Transform mTransform;

    float selfRotationSpeed = 90.0f;
    float coinRotationSpeed = 180.0f;

    void Awake() {
        mTransform = transform;
        Destroy(triggerGizmo);
    }
    
    void Update()
    {
        //mTransform.Rotate(0, selfRotationSpeed * Time.deltaTime, 0, Space.World);
        mTransform.rotation = Quaternion.Euler(0, selfRotationSpeed * Time.time, 0);
        
        if(orbitCoins != null)
        {
            orbitCoins.rotation = Quaternion.Euler(0, coinRotationSpeed * Time.time, 0);
            //orbitCoins.Rotate(0, coinRotationSpeed * Time.deltaTime, 0, Space.World);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            GameController.state.OnCoinsCollected(value, transform.position);
            Destroy(gameObject);
            Instantiate(explosionPrefab, mTransform.position, Quaternion.identity);
        }
    }
}
