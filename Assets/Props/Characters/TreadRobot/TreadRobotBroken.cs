using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TreadRobotBroken : MonoBehaviour
{
    public Transform head;
    public NavMeshAgent agent;
    public Transform walkArea;
    public ParticleSystem sparks;
    public AudioSource shortCircuitSound;
    public AudioSource movingSound;

    float headRotXMin = 20.0f;
    float headRotXMax = -25.0f;
    float headRotYMin = -27.0f;
    float headRotYMax = 27.0f;
    float headRotZMin = 16.0f;
    float headRotZMax = -16.0f;

    public float moveSoundMaxVol = 0.25f;

    IEnumerator Start()
    {
        while(true)
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    yield return StartCoroutine(Wander());
                    break;
                case 1:
                    yield return StartCoroutine(Lament());
                    break;
                case 2:
                    yield return StartCoroutine(Sulk());
                    break;
            }

            yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
        }
    }

    IEnumerator Wander()
    {
        var rot1 = head.localRotation;
        var rot2 = Quaternion.Euler(0, 0, 0);

        yield return StartCoroutine(Util.Blend(1.0f, t => {
            t = Curve.SmoothStepInSteep(t);
            head.localRotation = Quaternion.Slerp(rot1, rot2, t);
        }));

        var rand = Random.insideUnitCircle;
        var rand3 = new Vector3(rand.x, 0, rand.y);
        var randPos = walkArea.position + rand3 * 6.0f;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randPos, out hit, 12, 1))
        {
            if(agent.SetDestination(hit.position))
            {
                yield return null;

                while(!agent.isStopped && agent.remainingDistance >= 0.2f)
                {
                    movingSound.volume = (agent.velocity.magnitude / agent.speed) * moveSoundMaxVol;
                    yield return null;
                }

                movingSound.volume = 0;
                agent.isStopped = true;
                agent.ResetPath();
            }
        }
    }

    IEnumerator Lament()
    {
        var bodyRot1 = transform.localRotation;
        var bodyRot2 = Quaternion.Euler(0, Random.value * 360f, 0) * bodyRot1;
        var headRot1 = head.localRotation;
        var headRot2 = Quaternion.Euler(headRotXMax, headRotYMin * 0.2f, headRotZMin);

        yield return StartCoroutine(Util.Blend(1.0f, t => {
            t = Curve.SmoothStepInSteep(t);
            head.localRotation = Quaternion.Slerp(headRot1, headRot2, t);
            transform.localRotation = Quaternion.Slerp(bodyRot1, bodyRot2, t);
        }));

        yield return StartCoroutine(Util.Blend(7.0f, t => {
            t = -Curve.Cos(t * 2.0f) * 0.5f + 0.5f;
            head.localRotation = Quaternion.Euler(headRotXMax, Mathf.Lerp(headRotYMin * 0.2f, headRotYMax * 0.2f, t), Mathf.Lerp(headRotZMin, headRotZMax, t));
        }));
    }

    IEnumerator Sulk()
    {
        var bodyRot1 = transform.localRotation;
        var bodyRot2 = Quaternion.Euler(0, Random.value * 360f, 0) * bodyRot1;
        var headRot1 = head.localRotation;
        var headRot2 = Quaternion.Euler(headRotXMin, 0, 0);

        yield return StartCoroutine(Util.Blend(1.0f, t => {
            t = Curve.SmoothStepInSteep(t);
            head.localRotation = Quaternion.Slerp(headRot1, headRot2, t);
            transform.localRotation = Quaternion.Slerp(bodyRot1, bodyRot2, t);
        }));

        int shakes = Random.Range(2, 5);

        for (int i = 0; i != shakes; ++i)
        {
            yield return new WaitForSeconds(Random.Range(0.3f, 1.0f));

            sparks.Play();
            shortCircuitSound.Play();

            int jitters = Random.Range(3, 5);
            float len = jitters * 0.13f;

            yield return StartCoroutine(Util.Blend(len, t => {
                float j = Curve.Sine(t * jitters) * 0.5f + 0.5f;
                head.localRotation = Quaternion.Euler(headRotXMin, Mathf.Lerp(-8.0f, 8.0f, j), Mathf.Lerp(-2.8f, 2.8f, j));
            }));
        }
    }
}
