using UnityEngine;
using System.Collections;

public class DataTransfer : MonoBehaviour
{
    public float scrollSpeed = 2.0f;
    public MeshRenderer rend;
    private Material _mat;
    
    Vector2 offset = Vector2.zero;

    void Awake()
    {
        _mat = rend.material;
    }
    
    void Update()
    {
        offset.x = Mathf.Repeat(offset.x - Time.deltaTime * scrollSpeed, 1.0f);
        _mat.SetTextureOffset("_MainTex", offset);
    }
}
