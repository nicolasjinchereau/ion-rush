using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundRobotBroken : MonoBehaviour
{
    public Transform head;
    public ParticleSystem sparks;
    public AudioSource shortCircuitSound;

    int lastRotIndex = -1;

    Quaternion[] headRotations = new Quaternion[]{
        Quaternion.Euler(27.7878971f, 296.837097f, 352.052612f),
        Quaternion.Euler(47.6896057f, 39.758461f, 9.69193459f),
        Quaternion.Euler(9.12210274f, 316.914978f, 349.618927f),
        Quaternion.Euler(2.96257234f, 37.5667152f, 28.8447189f),
        Quaternion.Euler(350.955322f, 82.3678665f, 356.874969f),
        Quaternion.Euler(34.4337273f, 9.93012142f, 324.746887f)
    };

    IEnumerator Start()
    {
        while(true)
        {
            int tries = 0;
            int i = lastRotIndex;

            while(i == lastRotIndex && tries++ != 10)
                i = Random.Range(0, headRotations.Length);

            var rot1 = head.localRotation;
            var rot2 = headRotations[i];

            sparks.Play();
            shortCircuitSound.Play();

            yield return StartCoroutine(Util.Blend(0.3f, t => {
                t = Curve.SmoothStepInSteep(t);
                head.localRotation = Quaternion.Slerp(rot1, rot2, t);
            }));

            yield return new WaitForSeconds(Random.Range(0.3f, 1.0f));
        }
    }
}
