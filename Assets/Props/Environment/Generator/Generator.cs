using UnityEngine;
using System.Collections;

public class Generator : MonoBehaviour
{
    public ParticleSystem smoke;
    public ParticleSystem sparks;
    public Transform lookAtCam;
    public GameObject shortCircuitLightning1;
    public GameObject shortCircuitLightning2;
    public GameObject shortCircuitLightning3;

    public void BreakDown()
    {
        StartCoroutine(_breakDown());
    }

    public void StopBreakDown()
    {
        StopAllCoroutines();
        if (shortCircuitLightning1) shortCircuitLightning1.SetActive(false);
        if (shortCircuitLightning2) shortCircuitLightning2.SetActive(false);
        if (shortCircuitLightning3) shortCircuitLightning3.SetActive(false);
        smoke.EnableEmission(false);
    }

    IEnumerator _activateRandomly(GameObject obj, float forSeconds)
    {
        float finish = Time.time + forSeconds;

        while(Time.time < finish)
        {
            yield return new WaitForSeconds(Random.value * 0.5f);
            obj.SetActive(true);
            yield return new WaitForSeconds(Random.value * 0.5f);
            obj.SetActive(false);
        }
    }

    IEnumerator _breakDown()
    {
        if (shortCircuitLightning1) StartCoroutine(_activateRandomly(shortCircuitLightning1, 3.0f));
        if (shortCircuitLightning2) StartCoroutine(_activateRandomly(shortCircuitLightning2, 3.0f));
        if (shortCircuitLightning3) StartCoroutine(_activateRandomly(shortCircuitLightning3, 3.0f));

        smoke.EnableEmission(true);

        SharedSounds.explosion.Play(0.6f);
        SharedSounds.shortCircuit.Play(0.5f);
        
        for(int i = 0; i < 20; ++i)
        {
            Vector3 dir = Random.onUnitSphere;

            if(dir.y < 0)
                dir.y = - dir.y;

            sparks.transform.forward = dir;
            sparks.Emit(Random.Range(10, 15));
        }

        for(int i = 0; i < 8; ++i)
        {
            var emitParams = new ParticleSystem.EmitParams();
            emitParams.position = transform.position + Random.onUnitSphere * 1.1f;
            emitParams.velocity = Vector3.up;
            emitParams.startSize = 3.0f;
            emitParams.startLifetime = 10.0f;
            emitParams.startColor = Color.white;
            emitParams.rotation = Random.value * 360.0f;
            emitParams.angularVelocity = (Random.value * 2.0f - 1.0f) * 30.0f;

            var rateOverTime = smoke.emission.rateOverTime;
            var count = Random.Range(rateOverTime.constantMin, rateOverTime.constantMax);

            smoke.Emit(emitParams, Mathf.RoundToInt(count));
        }

        yield break;
    }
}
