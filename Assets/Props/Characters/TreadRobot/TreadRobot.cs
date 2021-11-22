using UnityEngine;
using System.Collections;

public class TreadRobot : MonoBehaviour
{
    public Transform body;
    public Transform head;
    public Grabber grabber;

    public enum State
    {
        None,
        Working,
        Looking,
        Manual,
    }

    private IEnumerator _stateBlock = null;
    private State _prevState = State.None;
    private State _state = State.None;

    public State defaultState = State.Working;

    public float workingMaxHeadRotation = 20.0f;
    public float workingHeadRotationSpeed = 80.0f;
    public float workingMinLookTime = 1.0f;
    public float workingMaxLookTime = 4.0f;

    public float lookingMaxPlayerDist = 100.0f;

    public State state
    {
        get { return _state; }
        set {
            _prevState = _state;
            _state = value;

            switch(_state)
            {
                case State.Working:
                    _stateBlock = WorkingState();
                    break;

                case State.Looking:
                    _stateBlock = LookingState();
                    break;

                case State.Manual:
                    _stateBlock = null;
                    break;
            }

            if(_stateBlock != null && !_stateBlock.MoveNext())
                _stateBlock = null;
        }
    }

    void Start()
    {
        state = defaultState;
    }

    void Update()
    {
        if(_stateBlock != null && !_stateBlock.MoveNext())
            _stateBlock = null;
    }
    
    IEnumerator WorkingState()
    {
        while(!Player.exists)
            yield return null;
        
        while(Quaternion.Angle(body.rotation, head.rotation) > 0.001f)
        {
            head.rotation = Quaternion.RotateTowards(head.rotation, body.rotation, 180.0f * Time.deltaTime);
            yield return null;
        }
        
        while(true)
        {
            float finish = Time.time + Random.Range(workingMinLookTime, workingMaxLookTime);
            while(Time.time <= finish)
                yield return null;

            Quaternion goalRot = Quaternion.Euler(0, workingMaxHeadRotation, 0) * body.rotation;
            while(Quaternion.Angle(goalRot, head.rotation) > 1.0f)
            {
                head.rotation = Quaternion.RotateTowards(head.rotation, goalRot, workingHeadRotationSpeed * Time.deltaTime);
                yield return null;
            }

            finish = Time.time + Random.Range(workingMinLookTime, workingMaxLookTime);
            while(Time.time <= finish)
                yield return null;

            goalRot = Quaternion.Euler(0, -workingMaxHeadRotation, 0) * body.rotation;
            while(Quaternion.Angle(goalRot, head.rotation) > 1.0f)
            {
                head.rotation = Quaternion.RotateTowards(head.rotation, goalRot, workingHeadRotationSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }

    IEnumerator LookingState()
    {
        while(!Player.exists)
            yield return null;

        while(true)
        {
            Vector3 playerPos = Player.position + Vector3.up * 0.5f;
            Vector3 lookDir = playerPos - head.position;

            RaycastHit hit;
            int layerMask = (1 << Player.that.playerLayer) | (1 << LayerMask.NameToLayer("Wall"));

            if(lookDir.magnitude > lookingMaxPlayerDist
                || !Physics.Raycast(head.position, lookDir, out hit, 1000.0f, layerMask)
                || hit.transform.gameObject.layer != Player.that.playerLayer)
            {
                lookDir = body.forward;
            }
            else
            {
                lookDir.y = 0;
            }

            head.rotation = Quaternion.RotateTowards(head.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 90.0f);

            yield return null;
        }
    }
}
