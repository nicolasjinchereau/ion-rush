using UnityEngine;
using System.Collections;

public class LoweringDoor : Useable
{
    public Transform door;
    public float lowerAmount = 1.0f;
    private Vector3 doorPos;

    public float doorSpeed = 1.0f;
    private float doorState = 0.0f;
    
    public Texture2D useButtonTexture;

    private bool _isOpen = false;
    public bool isOpen
    {
        set
        {
            if(_isOpen != value)
            {
                doorSound.Play();
            }

            _isOpen = value;
        }

        get { return _isOpen; }
    }

    public AudioSource doorSound;

    public override void OnAwake()
    {
        base.OnAwake();
        doorSound.ignoreListenerVolume = true;
    }

    void Start()
    {
        doorPos = door.transform.localPosition;
        doorState = 0.0f;
    }

    void Update()
    {
        if(_isOpen)
        {
            if(doorState < 1.0f)
                doorState = Mathf.Min(doorState + Time.deltaTime * doorSpeed, 1.0f);
        }
        else
        {
            if(doorState > 0.0f)
                doorState = Mathf.Max(doorState - Time.deltaTime * doorSpeed, 0.0f);
        }

        door.transform.localPosition = doorPos + Vector3.down * (lowerAmount * doorState);
    }

    public override void OnUseStart()
    {
        
    }

    public override void OnUseFinish()
    {
        
    }

    public override int OnAction()
    {
        isOpen = !isOpen;
        return 1;
    }
}
