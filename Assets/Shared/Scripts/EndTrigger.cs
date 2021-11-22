using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    public LevelEnd levelEnd;
    public Collider trigger;
    public DoorBlocker blocker;
    public Camera finalCamera;
    public TweenPath playerDriftPath;
    public ParticleSynthesizer particleSynthesizer;

    private IEnumerator Start()
    {
        yield return null;
        CameraController.that.maxX = 98.5f;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            StartCoroutine(EndGame());
        }
    }

    public void OnGearCollected()
    {
        StartCoroutine(EndGame());
    }

    IEnumerator EndGame()
    {
        Vector3 playerVelocity = Player.that.body.velocity;

        Player.that.enabled = false;
        Player.that.anim.CrossFade("Default");
        Player.that.body.constraints = RigidbodyConstraints.FreezeRotation;
        Player.that.body.useGravity = false;
        Player.that.body.velocity = playerVelocity;

        trigger.enabled = false;
        levelEnd.bigDoors.isOpen = false;
        blocker.col.isTrigger = false;
        CameraController.that.ShowView(finalCamera, 3.0f, Curve.SmoothStepIn);

        // wait for doors to close
        yield return new WaitForSeconds(2.5f);

        Player.that.body.velocity = Vector3.zero;
        var playerStartScale = Player.that.transform.localScale;

        StartCoroutine(Util.Blend(40.0f, t => {
            var ts = Curve.InLinear(t);
            Player.that.transform.localScale = Vector3.Lerp(playerStartScale, new Vector3(0.001f, 0.001f, 0.001f), ts);

            var tp = Curve.OutPowerInv(t, 2.0f);
            Player.position = playerDriftPath.SamplePosition(tp);
            Player.rotation = playerDriftPath.SampleRotation(tp);
        }));

        particleSynthesizer.StopSynthesis();

        yield return new WaitForSeconds(10.0f);

        // show result screen
        levelEnd.playerDidExit = true;

        yield break;
    }
}
