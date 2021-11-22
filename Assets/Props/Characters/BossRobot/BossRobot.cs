using UnityEngine;
using System.Collections;

public class BossRobot : MonoBehaviour
{
    public Transform xf;
    public Transform head;
    public Transform model;

    public enum State
    {
        WatchPlayer,
        LookForward,
        Manual,
    }

    public State state = State.Manual;

    void Awake()
    {
        //model.animation.CrossFade("Idle");
    }

    public void ResetHead()
    {
        head.rotation = xf.rotation;
        state = State.Manual;
    }

    public static void RotateHead(Transform body, Transform head)
    {
        if (Player.exists)
        {
            Vector3 playerPos = Player.that.transform.position + Vector3.up * 0.5f;
            Vector3 playerDir = playerPos - head.position;

            RaycastHit hit;
            int layerMask = (1 << Player.that.playerLayer) | (1 << LayerMask.NameToLayer("Wall"));

            if (!Physics.Raycast(head.position, playerDir, out hit, 1000.0f, layerMask)
            || hit.transform.gameObject.layer != Player.that.playerLayer)
            {
                playerDir = body.forward;
            }
            else
            {
                playerDir.y = 0;

                if (Vector3.Dot(playerDir, body.forward) < 0.4f)
                    playerDir = body.forward;
            }

            head.rotation = Quaternion.RotateTowards(head.rotation, Quaternion.LookRotation(playerDir), Time.deltaTime * 90.0f);
        }
    }
    void Update()
    {
        switch(state)
        {
            case State.WatchPlayer:
                RotateHead(xf, head);
                break;

            case State.LookForward:
                if(Quaternion.Angle(xf.rotation, head.rotation) > 0.5f)
                    head.rotation = Quaternion.RotateTowards(head.rotation, xf.rotation, 180 * Time.deltaTime);
                break;

            case State.Manual:
                
                break;
        }
    }
}
