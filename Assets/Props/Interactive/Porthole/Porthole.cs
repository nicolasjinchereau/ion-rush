using UnityEngine;
using System.Collections;

public class Porthole : Useable
{
    private float doorOpenOffset = 0.51f;
    public bool doorsOpened = false;

    public GameObject leftDoor;
    public GameObject rightDoor;

    public AudioSource openSound;

    public void Update()
    {
        if(doorsOpened)
        {
            Vector3 lpos = leftDoor.transform.localPosition;
            lpos.z = Mathf.Max(lpos.z - Time.deltaTime * 2.0f, -doorOpenOffset);
            leftDoor.transform.localPosition = lpos;

            Vector3 rpos = rightDoor.transform.localPosition;
            rpos.z = Mathf.Min(rpos.z + Time.deltaTime * 2.0f, doorOpenOffset);
            rightDoor.transform.localPosition = rpos;
        }
        else
        {
            Vector3 lpos = leftDoor.transform.localPosition;
            lpos.z = Mathf.Min(lpos.z + Time.deltaTime * 2.0f, 0);
            leftDoor.transform.localPosition = lpos;

            Vector3 rpos = rightDoor.transform.localPosition;
            rpos.z = Mathf.Max(rpos.z - Time.deltaTime * 2.0f, 0);
            rightDoor.transform.localPosition = rpos;
        }
    }

    public override int OnAction()
    {
        openSound.Play();
        doorsOpened = !doorsOpened;
        return 1;
    }
}
