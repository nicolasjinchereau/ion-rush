using UnityEngine;
using System.Collections;

public class SpinningGear : MonoBehaviour
{
    public float speed = 100.0f;
    
    Rigidbody body;
    
    void Awake() {
        body = GetComponent<Rigidbody>();
    }
    
    void FixedUpdate() {
        body.MoveRotation(Quaternion.Euler(0, speed * Time.deltaTime, 0) * body.rotation);
    }
}
