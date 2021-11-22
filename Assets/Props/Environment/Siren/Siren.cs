using UnityEngine;
using System.Collections;

public class Siren : MonoBehaviour {

    public Transform lightsPivot;

    bool _on = false;

    public bool on
    {
        get { return _on; }
        set
        {
            _on = value;
            lightsPivot.gameObject.SetActive(value);
        }
    }

    void Update()
    {
        if(_on)
            lightsPivot.transform.Rotate(0, 0, Time.deltaTime * 180.0f, Space.Self);
    }
}
