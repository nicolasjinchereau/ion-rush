using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadRobotBroken : MonoBehaviour
{
    public AudioSource rollingSound;
    public AudioSource hitSound;
    public ParticleSystem sparks;

    float rollSoundMaxVol = 0.5f;
    Vector3 startPos;

    IEnumerator Start()
    {
        startPos = transform.position;

        while(true)
        {
            float x = Random.value;
            float backupTime = Mathf.Lerp(1.0f, 2.5f, x);
            float backupDist = Mathf.Lerp(0.4f, 1.0f, x);
            float ramTime = Mathf.Lerp(0.4f, 0.1f, x);

            var backedUpPos = startPos + Vector3.left * backupDist;

            yield return StartCoroutine(Util.Blend(backupTime, t => {
                transform.position = Vector3.Lerp(startPos, backedUpPos, t);
                rollingSound.volume = Curve.ArcOct(t) * rollSoundMaxVol;
            }));

            rollingSound.volume = 0;
            
            yield return new WaitForSeconds(0.3f);

            yield return StartCoroutine(Util.Blend(ramTime, t => {
                transform.position = Vector3.Lerp(backedUpPos, startPos, Curve.InQuad(t));
                rollingSound.volume = Curve.InQuad(t) * rollSoundMaxVol;
            }));

            rollingSound.volume = 0;
            hitSound.Play();
            sparks.Play();

            float pauseTime = Random.Range(0.5f, 1.0f);
            yield return new WaitForSeconds(pauseTime);
        }
    }
}
