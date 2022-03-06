using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorTileHatched : MonoBehaviour
{
    public BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider.enabled = true;
    }
}
