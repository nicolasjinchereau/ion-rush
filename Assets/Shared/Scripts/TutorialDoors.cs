using UnityEngine;
using System.Collections;

public class TutorialDoors : MonoBehaviour
{
    public BigDoors leftDoors;
    public BigDoors rightDoors;
    public PassLight passLight;
    public Collider threshold;
    public Transform spawn;

    public bool didCrossThreshold = false;

    public bool doorsOpened
    {
        set
        {
            leftDoors.isOpen = value;
            rightDoors.isOpen = value;
        }

        get { return leftDoors.isOpen; }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player"
        && !didCrossThreshold
        && other.transform.position.x > this.transform.position.x)
        {
            didCrossThreshold = true;
            StartCoroutine(_closeDoor());
        }
    }

    IEnumerator _closeDoor()
    {
        threshold.isTrigger = false;

        float minX1 = CameraController.that.transform.position.x;
        float minX2 = transform.position.x + 2.5f;

        if(minX1 < minX2)
        {
            yield return StartCoroutine(Util.Blend(3.0f, (float t)=>{
                //t = Curve.SmoothStepIn(t);
                t = 1.0f - (1.0f - t) * (1.0f - t);
                CameraController.that.minX = Mathf.Lerp(minX1, minX2, t);
            }));
        }

        leftDoors.isOpen = false;
        rightDoors.isOpen = false;
    }
}
