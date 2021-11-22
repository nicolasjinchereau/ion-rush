using UnityEngine;

public class UseTrigger : MonoBehaviour 
{
    public Useable target;

    private bool inUse = false;
    private Renderer _renderer;
    private float _flashStartTime;
    private Color _exclaimColor = Color.white;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        if(_renderer != null)
        {
            _flashStartTime = Time.time;
            _exclaimColor.a = 1;
            _renderer.material.SetColor("_Color", _exclaimColor);
        }
    }

    private void OnEnable()
    {
        if(_renderer != null)
        {
            _flashStartTime = Time.time;
            _exclaimColor.a = 1;
            _renderer.material.SetColor("_Color", _exclaimColor);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            inUse = true;
            GameController.state.OnUseTriggerActivated(this);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            inUse = false;
            _flashStartTime = Time.time;
            GameController.state.OnUseTriggerDeactivated(this);
        }
    }

    void Update()
    {
        if(inUse)
        {
            if(_renderer != null && _exclaimColor.a < 1.0f)
            {
                _exclaimColor.a = Mathf.Min(_exclaimColor.a + Time.deltaTime * 4.0f, 1.0f);
                _renderer.material.SetColor("_Color", _exclaimColor);
            }
        }
        else
        {
            if(_renderer != null)
            {
                float time = Time.time - _flashStartTime;
                _exclaimColor.a = Mathf.Cos(time * 4.0f) * 0.5f + 0.5f;
                _renderer.material.SetColor("_Color", _exclaimColor);
            }
        }
    }
}
