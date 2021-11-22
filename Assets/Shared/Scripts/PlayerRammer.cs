using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRammer : MonoBehaviour
{
    public Rigidbody rammer;
    public Transform ramArea;
    public Renderer ramAreaRenderer;

    public float ramRobotSpeed = 2.0f;
    public float backupRobotSpeed = 1.0f;

    bool isRamming = false;
    Vector3 startPos;
    Vector3 endPos;
    int boxLayer = 0;

    void Start()
    {
        startPos = rammer.position;
        endPos = ramArea.position + ramArea.forward * (ramArea.lossyScale.z * 0.5f - rammer.GetComponentInChildren<Collider>().bounds.size.z * 0.5f);
        endPos.y = startPos.y;
        ramAreaRenderer.enabled = false;
        boxLayer = LayerMask.NameToLayer("Box");
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player" && !isRamming) {
            StartCoroutine(RamPlayer());
        }
    }

    IEnumerator RamPlayer()
    {
        isRamming = true;

        var dist = Vector3.Distance(startPos, endPos);
        var duration1 = dist / ramRobotSpeed;
        var duration2 = dist / backupRobotSpeed;

        yield return StartCoroutine(Util.Blend(duration1, t => {
            rammer.MovePosition(Vector3.Lerp(startPos, endPos, t));
        }));

        yield return StartCoroutine(Util.Blend(duration2, t => {
            rammer.MovePosition(Vector3.Lerp(endPos, startPos, t));
        }));

        isRamming = false;
    }
}
