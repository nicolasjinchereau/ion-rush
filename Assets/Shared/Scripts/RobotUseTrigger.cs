using UnityEngine;
using System.Collections;

public class RobotUseTrigger : MonoBehaviour
{
    public Useable[] targets;

    public Renderer screen;
    private Color _screenColor = Color.black;

    private Renderer _renderer;
    private float _startTime;
    private Color _iconColor = Color.white;
    private bool _inUse = false;

    public bool InUse {
        get { return _inUse; }
        set {
            if(_inUse != value)
            {
                _inUse = value;
                if(_inUse) {
                    SharedSounds.activate.Play();
                }
                else {
                    _startTime = Time.time;
                    SharedSounds.deactivate.Play();
                }
            }
        }
    }

    public void DoAction()
    {
        foreach(Useable target in targets)
            target.PerformAction();
    }

    void Awake()
    {
        _inUse = false;
        _iconColor.a = 1;
        _renderer = GetComponent<Renderer>();
        _renderer.material.SetColor("_Color", _iconColor);

        if(screen != null)
        {
            _screenColor = Color.black;
            screen.material.SetColor("_Color", _screenColor);
        }

        _startTime = Time.time;
    }
    
    private void Update()
    {
        if(!_inUse)
        {
            float time = Time.time - _startTime;
            _iconColor.a = Mathf.Cos(time * 4.0f) * 0.5f + 0.5f;
            _renderer.material.SetColor("_Color", _iconColor);
        }
        else if(_iconColor.a < 1.0f)
        {
            _iconColor.a = Mathf.Min(_iconColor.a + Time.deltaTime * 4.0f, 1.0f);
            _renderer.material.SetColor("_Color", _iconColor);
        }

        if(screen != null)
        {
            if(_inUse && _screenColor.a > 0)
            {
                _screenColor.a = Mathf.Max(_screenColor.a - Time.deltaTime * 10, 0);
                screen.material.SetColor("_Color", _screenColor);
            }
            else if(!_inUse && _screenColor.a < 1)
            {
                _screenColor.a = Mathf.Min(_screenColor.a + Time.deltaTime * 10, 1);
                screen.material.SetColor("_Color", _screenColor);
            }
        }
    }
}
