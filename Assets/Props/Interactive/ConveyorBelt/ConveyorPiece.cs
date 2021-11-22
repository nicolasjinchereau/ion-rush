using UnityEngine;
using System.Collections;

public class ConveyorPiece : MonoBehaviour
{
    public Transform mTransform;
    public Rigidbody mRigidbody;

    void Awake()
    {
        mTransform = transform;
        mRigidbody = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
    
    }
}
