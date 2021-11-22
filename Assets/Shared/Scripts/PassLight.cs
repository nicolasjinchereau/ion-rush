using UnityEngine;
using System.Collections;

public class PassLight : MonoBehaviour
{
    public Texture2D redTex;
    public Texture2D greenTex;

    private Material _mat;

    void Awake()
    {
        _mat = GetComponentInChildren<Renderer>().material;
    }

    private bool _isGreen = false;
    public bool isGreen
    {
        get { return _isGreen; }
        set
        {
            if(_isGreen != value)
            {
                _isGreen = value;
                _mat.SetTexture("_MainTex", _isGreen ? greenTex : redTex);
            }
        }
    }
}
