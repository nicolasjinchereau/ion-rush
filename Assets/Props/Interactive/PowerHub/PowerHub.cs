using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.InputSystem;

public class PowerHub : MonoBehaviour
{
    [Serializable]
    public class ShortCircuitEvent : UnityEvent
    {
    }

    public GameObject shortCircuitTrigger;
    public List<LightningBolt> powerTransferArcs;
    public List<ShortCircuit> shortCircuits;
    public List<ShortCircuit> objectShortCircuits;
    public List<GameObject> electricDischarges;
    public List<GameObject> electricDischargesForDoor;
    public List<ShortCircuit> shortCircuitsForDoor;

    public ShortCircuitEvent onShortCircuitBegin;
    public ShortCircuitEvent onShortCircuitEnd;
    
    public AudioClip electricSound;
    public AudioClip disasterSong;

    public Siren siren1;
    public Siren siren2;
    public ControlPanel controlPanel;
    public ConveyorBelt conveyorBelt1;
    public ConveyorBelt conveyorBelt2;
    public ConveyorBelt conveyorBelt3;
    public GameObject windowDebris;
    public GameObject windowExplosions;
    public BigDoors roomDoors1;
    public BigDoors roomDoors2;
    public TimedSurge[] timedSurges;
    public GroundTurret[] groundTurrets;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(DoShortCircuit(true));
        }
        else if (other.tag == "HeavyCrate")
        {
            StartCoroutine(DoShortCircuit(false));
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current.pKey.wasReleasedThisFrame)
        {
            StartCoroutine(DoShortCircuit(false));
        }
#endif
    }

    IEnumerator DoShortCircuit(bool isPlayer)
    {
        if (isPlayer) {
            Player.that.enabled = false;
        }

        shortCircuitTrigger.SetActive(false);

        MusicMixer.Stop(0.5f);

        CameraController.that.CameraShakeEnabled = true;
        CameraController.that.DoImpactShake();
        SharedSounds.explosion.Play();
        SharedSounds.shortCircuit.Play();

        // do flash/spark particles on conductors (1 in 3 chance)
        foreach (var sc in shortCircuits)
        {
            var delay = UnityEngine.Random.Range(0.0f, 3.0f);
            StartCoroutine(PlayAfterDelay(sc, delay));
        }

        // do electrical discharge thin arcs (1 in 2 chance)
        foreach (var ed in electricDischarges)
        {
            var delay = UnityEngine.Random.Range(0.0f, 3.0f);
            var duration = UnityEngine.Random.Range(0.5f, 1.0f);
            StartCoroutine(DoElectricDischarge(ed, delay, duration));
        }

        // do 3 flash/spark particles where colliding object hit the arcs
        foreach (var sc in objectShortCircuits)
        {
            var delay = UnityEngine.Random.Range(0.0f, 0.4f);
            StartCoroutine(PlayAfterDelay(sc, delay));
        }

        float minPowerdownLen = 1.0f;
        float maxPowerdownLen = 3.0f;

        // power down the 8 horizontal power arcs in random order
        foreach (var arc in powerTransferArcs)
        {
            var len = UnityEngine.Random.Range(minPowerdownLen, maxPowerdownLen);

            var currentArc = arc;
            var startScale = arc.scale;
            
            StartCoroutine(Util.Blend(len, t =>
            {
                t = Curve.InCube(t);
                arc.scale = Mathf.Lerp(startScale, startScale * 6.0f, t);
                arc.particleSizeScale = Mathf.Lerp(1.0f, 0.001f, t);
            }, () => arc.gameObject.SetActive(false)));
        }

        yield return new WaitForSeconds(2.0f);

        if (isPlayer)
        {
            Player.that.enabled = true;

            // player dying will turn the lights off
            Player.that.DrainBattery(100, BatteryDrainReason.PowerSurge);
        }
        else
        {
            StartCoroutine(Util.Blend(0.5f, t =>
            {
                RenderSettings.ambientLight = Color.Lerp(Defaults.ambientColor, Defaults.ambientOffColor, t);
                MainLight.Light.intensity = Mathf.Lerp(Defaults.lightIntensity, Defaults.lightOffIntensity, t);
            }));
        }

        electricDischargesForDoor[0].SetActive(true);
        shortCircuitsForDoor[0].Play();
        yield return new WaitForSeconds(0.25f);

        electricDischargesForDoor[0].SetActive(false);
        electricDischargesForDoor[1].SetActive(true);
        shortCircuitsForDoor[1].Play();
        yield return new WaitForSeconds(0.25f);

        electricDischargesForDoor[1].SetActive(false);

        // short circuit done
        siren1.on = true;
        siren2.on = true;
        controlPanel.SetOff();
        conveyorBelt1.enabled = false;
        conveyorBelt2.enabled = false;
        conveyorBelt3.enabled = false;
        windowDebris.SetActive(true);
        windowExplosions.SetActive(true);

        foreach (var turret in groundTurrets)
            turret.enabled = true;

        foreach (var surge in timedSurges)
            surge.enabled = true;

        yield return new WaitForSeconds(1.0f);

        roomDoors1.isOpen = true;
        roomDoors2.isOpen = true;

        yield return new WaitForSeconds(1.0f);

        if (onShortCircuitEnd != null)
            onShortCircuitEnd.Invoke();

        MusicMixer.PlayDelayed(disasterSong, 1.0f, 0, 0.75f, true, 0);
    }

    IEnumerator PlayAfterDelay(ShortCircuit sc, float delay)
    {
        if (delay > float.Epsilon)
            yield return new WaitForSeconds(delay);

        sc.Play();
    }

    IEnumerator DoElectricDischarge(GameObject go, float delay, float duration)
    {
        if (delay > float.Epsilon)
            yield return new WaitForSeconds(delay);

        Util.PlayClip(electricSound, SharedSounds.MixerGroup, 0.25f);
        go.SetActive(true);
        
        yield return new WaitForSeconds(duration);
        
        go.SetActive(false);
    }
}
