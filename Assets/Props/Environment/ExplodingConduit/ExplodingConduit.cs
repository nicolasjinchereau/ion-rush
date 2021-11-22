using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingConduit : MonoBehaviour
{
    public GameObject intactParts;
    public GameObject brokenParts;

    public LineRenderer arcRenderer;
    public ParticleSystem breakPoint;
    public ParticleSystem conduitEnd1;
    public ParticleSystem conduitEnd2;
    public Collider trigger;

    private void Awake()
    {
        intactParts.SetActive(true);
        brokenParts.SetActive(false);
        arcRenderer.enabled = false;
    }

    public void Explode()
    {
        StartCoroutine(DoExplosions());
    }

    IEnumerator DoExplosions()
    {
        CameraController.that.DoImpactShake(0.2f);
        SharedSounds.explosion.Play(0.7f);
        intactParts.SetActive(false);
        brokenParts.SetActive(true);
        breakPoint.Play();

        while (true)
        {
            if (Random.Range(0, 2) == 1)
            {
                conduitEnd1.Play(true);
                arcRenderer.enabled = true;
                yield return new WaitForSeconds(0.1f);
                conduitEnd2.Play(true);
            }
            else
            {
                conduitEnd2.Play(true);
                arcRenderer.enabled = true;
                yield return new WaitForSeconds(0.1f);
                conduitEnd1.Play(true);
            }

            yield return new WaitForSeconds(Random.Range(0.4f, 1.0f));

            arcRenderer.enabled = false;

            yield return new WaitForSeconds(Random.Range(2.0f, 3.0f));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            trigger.SetActive(false);
            Explode();
        }
    }
}
