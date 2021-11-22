using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum BargeState
{
    PausedAtStart,
    MovingForward,
    PausedAtEnd,
    MovingBackward
}

public class Barge : MonoBehaviour
{
    public BoxCollider boxCollider;
    public Rigidbody body;
    public Transform start;
    public Transform end;

    public float travelTime = 5.0f;
    public float pauseLength = 1.0f;
    public bool startMovingForward = true;

    Vector3 direction;
    Vector3 startPos;
    Quaternion startRot;
    Vector3 endPos;
    Quaternion endRot;
    
    BargeState state;
    float pauseStartTime;

    private void Awake()
    {
        direction = (end.position - start.position).normalized;

        startPos = start.position + direction * (end.localScale.y + boxCollider.size.y) * 0.5f;
        startRot = start.rotation;

        endPos = end.position - direction * (end.localScale.y + boxCollider.size.y) * 0.5f;
        endRot = end.rotation;

        Destroy(start.gameObject);
        Destroy(end.gameObject);
        start = null;
        end = null;
    }

    void Start()
    {
        state = startMovingForward ? BargeState.MovingForward : BargeState.MovingBackward;
        pauseStartTime = Time.time;
	}
	
	void FixedUpdate()
    {
        if(state == BargeState.PausedAtStart)
        {
            float elapsed = Time.time - pauseStartTime;
            
            if(elapsed >= pauseLength)
            {
                state = BargeState.MovingForward;
            }
        }
        else if(state == BargeState.MovingForward)
        {
            var t = Vector3.Distance(startPos, body.position) / Vector3.Distance(startPos, endPos);
            t += Time.deltaTime / travelTime;

            var pos = Vector3.Lerp(startPos, endPos, t);
            body.MovePosition(pos);

            var rot = Quaternion.Slerp(startRot, endRot, t);
            body.MoveRotation(rot);

            if(t >= 1.0f)
            {
                t = 1.0f;
                state = BargeState.PausedAtEnd;
                pauseStartTime = Time.time;
            }
        }
        else if(state == BargeState.PausedAtEnd)
        {
            float elapsed = Time.time - pauseStartTime;

            if(elapsed >= pauseLength)
            {
                state = BargeState.MovingBackward;
            }
        }
        else if(state == BargeState.MovingBackward)
        {
            var t = Vector3.Distance(startPos, body.position) / Vector3.Distance(startPos, endPos);
            t -= Time.deltaTime / travelTime;

            var pos = Vector3.Lerp(startPos, endPos, t);
            body.MovePosition(pos);

            var rot = Quaternion.Slerp(startRot, endRot, t);
            body.MoveRotation(rot);

            if(t <= 0.0f)
            {
                t = 0.0f;
                state = BargeState.PausedAtStart;
            }
        }
    }
}
