using UnityEngine;
using System.Collections;

public class SmallDoors : MonoBehaviour
{
    public Transform leftHinge;
    public Transform rightHinge;

    public Transform leftDoor;
    public Transform rightDoor;

    private Vector3 leftDoorPos;
    private Vector3 rightDoorPos;

    bool _prevDoorState = false;
    public bool _isOpen = false;
    public bool isOpen
    {
        set
        {
            //if(_isOpen != value)
            //{
            //    doorSound.Play();
            //}

            _isOpen = value;
        }

        get { return _isOpen; }
    }

    float doorState = 1.0f;
    float doorSpeed = 6.0f;

    public AudioSource doorSound;

    void Awake()
    {
        leftDoorPos = leftDoor.localPosition;
        rightDoorPos = rightDoor.localPosition;
        doorSound.ignoreListenerVolume = true;
    }

    void Update()
    {
        if (_prevDoorState != _isOpen)
        {
            doorSound.Play();
            _prevDoorState = _isOpen;
        }

        if (_isOpen)
        {
            if(doorState > 0.0f)
            {
                var smoothState = Curve.SmoothStepOutSteep(doorState);
                smoothState = Curve.SmoothStepOutSteep(smoothState);
                smoothState = Curve.SmoothStepOutSteep(smoothState);

                float acc = 0.9f * smoothState  + 0.1f;
                doorState = Mathf.Max(doorState - Time.deltaTime * acc * doorSpeed, 0.0f);
            }
        }
        else
        {
            if(doorState < 1.0f)
            {
                var smoothState = Curve.SmoothStepOutSteep(doorState);
                smoothState = Curve.SmoothStepOutSteep(smoothState);
                smoothState = Curve.SmoothStepOutSteep(smoothState);

                float acc = 0.9f * smoothState + 0.1f;
                doorState = Mathf.Min(doorState + Time.deltaTime * acc * doorSpeed, 1.0f);
            }
        }


        Vector3 p;
        
        p = leftDoor.transform.localPosition;
        p.x = leftHinge.localPosition.x + (leftDoorPos.x - leftHinge.localPosition.x) * doorState;
        leftDoor.transform.localPosition = p;

        p = rightDoor.transform.localPosition;
        p.x = rightHinge.localPosition.x - (rightHinge.localPosition.x - rightDoorPos.x) * doorState;
        rightDoor.transform.localPosition = p;
    }
}
