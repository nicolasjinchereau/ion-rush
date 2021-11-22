using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedSurge : MonoBehaviour
{
    public GameObject lightning;
    public Collider lightningCollider;
    public Transform burnMark;
    public AudioSource shortCircuitSound;
    public GameObject shortCircuitPrefab;

    public float initialDelay = 0.0f;
    public float activeDuration = 1.0f;
    public float activationDelay = 1.0f;

    bool canHit = false;

    private void Awake() {
        RotateBurnMarkRandomly();
    }

    private void OnEnable()
    {
        StartCoroutine(DoSurges());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        lightning.SetActive(false);
        lightningCollider.enabled = false;
    }

    IEnumerator DoSurges()
    {
        yield return new WaitForSeconds(initialDelay);
        
        while (true)
        {
            lightning.SetActive(true);
            lightningCollider.enabled = true;
            RotateBurnMarkRandomly();
            yield return new WaitForSeconds(activeDuration);
            lightning.SetActive(false);
            lightningCollider.enabled = false;
            yield return new WaitForSeconds(activationDelay);
        }
    }

    void RotateBurnMarkRandomly() {
        burnMark.Rotate(burnMark.forward, 360.0f * Random.value, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            var shortCircuit = Instantiate(shortCircuitPrefab, other.bounds.center, Quaternion.identity);
            shortCircuit.GetComponent<ShortCircuit>().Play(true);
            shortCircuitSound.Play();
            Player.that.DrainBattery(Difficulty.timedSurgeDamage, BatteryDrainReason.PowerSurge);
        }
    }
}
