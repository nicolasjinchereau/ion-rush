using UnityEngine;
using System.Collections;

public class UseAutoTriggerBox : MonoBehaviour 
{
    public Useable target;
    public bool oneShot = true;
    public bool playSounds = false;

    void Start()
    {
        Destroy(GetComponent<MeshRenderer>());
        Destroy(GetComponent<MeshFilter>());
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && (target.unused || !oneShot))
        {
            int ret = target.OnAction();
            if(ret == 1)
            {
                target.unused = false;

                if(playSounds)
                    SharedSounds.useButton.Play();
            }
            else if(ret == 0)
            {
                if(playSounds)
                    SharedSounds.error.Play();
            }
        }
    }
}
