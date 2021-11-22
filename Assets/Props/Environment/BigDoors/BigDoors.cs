using UnityEngine;
using System.Collections;

public class BigDoors : MonoBehaviour
{
    public Transform leftHinge;
    public Transform rightHinge;

    public Transform leftOuterDoor;
    public Transform leftInnerDoor;
    public Transform rightOuterDoor;
    public Transform rightInnerDoor;

    private Vector3 leftOuterDoorPos;
    private Vector3 leftInnerDoorPos;
    private Vector3 rightOuterDoorPos;
    private Vector3 rightInnerDoorPos;

    public bool _isOpen = false;
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

    float doorState = 1.0f;
    float doorSpeed = 6.0f;

    public AudioSource doorSound;

    void Awake()
    {
        leftOuterDoorPos = leftOuterDoor.localPosition;
        leftInnerDoorPos = leftInnerDoor.localPosition;
        rightOuterDoorPos = rightOuterDoor.localPosition;
        rightInnerDoorPos = rightInnerDoor.localPosition;
        doorSound.ignoreListenerVolume = true;
    }

    void Update()
    {
        if(_isOpen)
        {
            if(doorState > 0.0f)
            {
                float acc = 0.9f * Curve.SmoothStepOutSteep(doorState) + 0.1f;
                doorState = Mathf.Max(doorState - Time.deltaTime * acc * doorSpeed, 0.0f);
            }
        }
        else
        {
            if(doorState < 1.0f)
            {
                float acc = 0.9f * Curve.SmoothStepOutSteep(doorState) + 0.1f;
                doorState = Mathf.Min(doorState + Time.deltaTime * acc * doorSpeed, 1.0f);
            }
        }


        Vector3 p;
        
        p = leftOuterDoor.transform.localPosition;
        p.x = leftHinge.localPosition.x + (leftOuterDoorPos.x - leftHinge.localPosition.x) * doorState;
        leftOuterDoor.transform.localPosition = p;

        p = leftInnerDoor.transform.localPosition;
        p.x = leftHinge.localPosition.x + (leftInnerDoorPos.x - leftHinge.localPosition.x) * doorState;
        leftInnerDoor.transform.localPosition = p;

        p = rightOuterDoor.transform.localPosition;
        p.x = rightHinge.localPosition.x - (rightHinge.localPosition.x - rightOuterDoorPos.x) * doorState;
        rightOuterDoor.transform.localPosition = p;

        p = rightInnerDoor.transform.localPosition;
        p.x = rightHinge.localPosition.x - (rightHinge.localPosition.x - rightInnerDoorPos.x) * doorState;
        rightInnerDoor.transform.localPosition = p;
    }
}
