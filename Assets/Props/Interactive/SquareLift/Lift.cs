using UnityEngine;
using System.Collections;

public class Lift : Useable
{
    public Rigidbody rb;
    public Transform xfRaised;
    public Transform xfLowered;
    public AudioSource startSound;
    public AudioSource movingSound;
    public AudioSource stopSound;

    public bool isRaised = false;
    public float liftMaxSpeed = 1.0f;

    private Vector3 liftPos0;
    private Quaternion liftRot0;
    private Vector3 liftPos1;
    private Quaternion liftRot1;
    private float liftState;

    private float liftHeight;
    private float accelDuration  = 1.0f; //2.0f;
    //private float steadyRate     = 0.5f;
    private float accelRate;
    private float accelDist;
    private float steadyDist;
    private float steadyDuration;
    private float totalDuration;
    private float normalAccelDuration;
    private float normalSteadyDuration;

    private IEnumerator liftRoutine = null;

    void Start()
    {
        liftPos0 = transform.position;
        liftRot0 = transform.rotation;
        liftPos1 = liftPos0 + (xfRaised.position - xfLowered.position);
        liftRot1 = (Quaternion.Inverse(xfLowered.rotation) * xfRaised.rotation) * liftRot0;
        liftState = isRaised ? 1.0f : 0.0f;

        liftHeight = liftPos1.y - liftPos0.y;
        accelRate = liftMaxSpeed / accelDuration;
        accelDist = CalcDist(accelRate, accelDuration);
        steadyDist = liftHeight - accelDist * 2.0f;
        steadyDuration = steadyDist / liftMaxSpeed;
        totalDuration = accelDuration + accelDuration + steadyDuration;
        normalAccelDuration = accelDuration / totalDuration;
        normalSteadyDuration = steadyDuration / totalDuration;

        UpdateLiftPos();
    }
    
    void UpdateLiftPos()
    {
        transform.position = Vector3.Lerp(liftPos0, liftPos1, liftState);
        transform.rotation = Quaternion.Slerp(liftRot0, liftRot1, liftState);
    }

    public void Update()
    {
        if(liftRoutine != null) {
            UpdateLiftPos();
        }
    }

    float CalcDist(float acceleration, float time, float initialVel = 0)
    {
        return initialVel * time + 0.5f * acceleration * time * time;
    }

    float CalcLiftState(float t)
    {
        if(t < normalAccelDuration)
        {
            float nt = t / normalAccelDuration;
            float dt = nt;
            float S_t = dt * dt / accelDuration;
            float p = accelDist * S_t * accelDuration;
            return p / liftHeight;
        }
        else if(t >= (1.0f - normalAccelDuration))
        {
            float nt = (t - (1.0f - normalAccelDuration)) / normalAccelDuration;
            float dt = 1.0f - nt;
            float S_t = dt * dt / accelDuration;
            float p = accelDist + steadyDist + accelDist * (1.0f - S_t * accelDuration);
            return p / liftHeight;
        }
        else
        {
            float nt = (t - normalAccelDuration) / normalSteadyDuration;
            float p = accelDist + steadyDist * nt;
            return p / liftHeight;
        }
    }

    IEnumerator RaiseLift()
    {
        yield return new WaitForSeconds(0.5f);

        startSound.Play();
        movingSound.Play();

        yield return StartCoroutine(Util.Blend(totalDuration, (float t)=>{
            liftState = CalcLiftState(t);
        }));
        
        movingSound.Stop();
        stopSound.Play();

        liftRoutine = null;
    }

    IEnumerator LowerLift()
    {
        yield return new WaitForSeconds(0.5f);

        startSound.Play();
        movingSound.Play();

        yield return StartCoroutine(Util.Blend(totalDuration, (float t)=>{
            liftState = CalcLiftState(1.0f - t);
        }));

        movingSound.Stop();
        stopSound.Play();

        liftRoutine = null;
    }

    public override void OnUseStart()
    {
        
    }

    public override void OnUseFinish()
    {
        
    }

    public override int OnAction()
    {
        if(liftRoutine == null)
        {
            isRaised = !isRaised;

            if(isRaised)
                StartCoroutine(liftRoutine = RaiseLift());
            else
                StartCoroutine(liftRoutine = LowerLift());

            return 1;
        }

        return -1;
    }
}
