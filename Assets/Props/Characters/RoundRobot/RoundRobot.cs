using UnityEngine;
using System.Collections;
using System.Threading;

public class RoundRobot : MonoBehaviour
{
    public Transform body;
    public Transform head;
    public DataTransfer dataTransfer;
    public AudioSource dataSound;
    public MeshRenderer irisRenderer;

    CancellationTokenSource cancelSource = new CancellationTokenSource();

    public enum State
    {
        None,
        Working,
        Looking,
        Surprised,
        Manual,
    }

    private State _prevState = State.None;
    private State _state = State.None;

    public State defaultState = State.Working;
    
    public State state
    {
        get
        {
            return _state;
        }
        set
        {
            cancelSource.Cancel();
            cancelSource = new CancellationTokenSource();
            
            _prevState = _state;
            _state = value;

            switch(_state)
            {
                case State.Working:
                    this.StartCoroutine(WorkingState(), cancelSource.Token);
                    break;

                case State.Looking:
                    this.StartCoroutine(LookingState(), cancelSource.Token);
                    break;

                case State.Surprised:
                    this.StartCoroutine(SurprisedState(), cancelSource.Token);
                    break;

                case State.Manual:
                    break;
            }
        }
    }
    
    public bool dataTransferEnabled {
        get { return dataTransfer.gameObject.activeSelf; }
        set {
            dataTransfer.gameObject.SetActive(value);
            if(value && !dataSound.isPlaying)
                dataSound.Play();
            else if(!value && dataSound.isPlaying)
                dataSound.Stop();
        }
    }
    
    void Start()
    {
        state = defaultState;
    }

    IEnumerator WorkingState()
    {
        while (!Player.exists)
        {
            yield return null;
        }

        while(Quaternion.Angle(body.rotation, head.rotation) > 0.001f)
        {
            head.rotation = Quaternion.RotateTowards(head.rotation, body.rotation, 180.0f * Time.deltaTime);
            yield return null;
        }

        while(true)
        {
            float finish = Time.time + Random.Range(0.5f, 2.5f);
            while (Time.time <= finish)
            {
                yield return null;
            }

            Quaternion goalRot = Quaternion.Euler(0, 35.0f, 0) * body.rotation;
            while(Quaternion.Angle(goalRot, head.rotation) > 1.0f)
            {
                head.rotation = Quaternion.RotateTowards(head.rotation, goalRot, 90.0f * Time.deltaTime);
                yield return null;
            }

            finish = Time.time + Random.Range(0.5f, 2.5f);
            while (Time.time <= finish)
            {
                yield return null;
            }

            goalRot = Quaternion.Euler(0, -35.0f, 0) * body.rotation;
            while(Quaternion.Angle(goalRot, head.rotation) > 1.0f)
            {
                head.rotation = Quaternion.RotateTowards(head.rotation, goalRot, 90.0f * Time.deltaTime);
                yield return null;
            }
        }
    }

    IEnumerator LookingState()
    {
        while (!Player.exists)
        {
            yield return null;
        }

        while(true)
        {
            Vector3 playerPos = Player.position + Vector3.up * 0.5f;
            Vector3 lookDir = playerPos - head.position;

            RaycastHit hit;
            int layerMask = (1 << Player.that.playerLayer) | (1 << LayerMask.NameToLayer("Wall"));

            if(!Physics.Raycast(head.position, lookDir, out hit, 1000.0f, layerMask)
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

    IEnumerator SurprisedState()
    {
        Quaternion a = head.rotation;
        Quaternion b = Quaternion.LookRotation(Player.position - body.position, Vector3.up);
        
        Vector3 startPos = transform.position;

        var length = Quaternion.Angle(a, b) / 90.0f * 0.5f;

        var token = cancelSource.Token;

        yield return this.StartCoroutine(Util.Blend(length, (float t) =>
        {
            float th = Util.NormalizedClamp(t, 0.0f, 0.4f);
            head.rotation = Quaternion.Slerp(a, b, th);

            float tp = Util.NormalizedClamp(t, 0.35f, 0.8f);
            float y = startPos.y + Curve.ArcQuad(tp) * 0.4f;
            transform.position = new Vector3(startPos.x, y, startPos.z);
        }), token);

        this.StartCoroutine(LookingState(), token);
    }
}
