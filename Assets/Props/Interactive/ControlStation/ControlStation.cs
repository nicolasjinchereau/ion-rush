using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.Events;

[Serializable]
public class ControlStationEvent : UnityEvent
{
}

public enum EnableBehaviour
{
    Enable,
    Disable,
    Toggle
}

public class ControlStation : Useable
{
    public RoundRobot robot;
    public Siren siren;
    public Megaphone megaphone;

    public EnableBehaviour behaviour = EnableBehaviour.Enable;
    public List<GameObject> gameObjects = new List<GameObject>();
    public List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>();

    Coroutine activationSequence = null;
    
    public override void OnAwake()
    {
        base.OnAwake();
        Destroy(GetComponent<MeshRenderer>());
        Destroy(GetComponent<MeshFilter>());
    }

    public override void OnUseStart()
    {

    }

    public override void OnUseFinish()
    {

    }

    public override int OnAction()
    {
        if (activationSequence == null && this.unused)
        {
            activationSequence = StartCoroutine(ActivationSequence());
        }

        return 1;
    }

    IEnumerator ActivationSequence()
    {
        siren.on = true;
        yield return new WaitForSeconds(1.0f);

        megaphone.Buzz();

        yield return new WaitForSeconds(0.3f);

        robot.state = RoundRobot.State.Surprised;

        yield return new WaitForSeconds(2.0f);

        // look at control panel and do data transfer
        robot.state = RoundRobot.State.Manual;

        var startRotation = robot.head.rotation;
        var finishRotation = robot.transform.rotation;

        // look back at control panel
        yield return StartCoroutine(Util.Blend(0.25f, (float t) => {
            robot.head.rotation = Quaternion.Slerp(startRotation, finishRotation, t);
        }));

        robot.dataTransferEnabled = true;
        yield return new WaitForSeconds(1.0f);
        robot.dataTransferEnabled = false;
        // data transfer end

        foreach (var go in gameObjects)
        {
            if (go)
            {
                if (behaviour == EnableBehaviour.Enable)
                    go.SetActive(true);
                else if (behaviour == EnableBehaviour.Disable)
                    go.SetActive(false);
                else if (behaviour == EnableBehaviour.Toggle)
                    go.SetActive(!go.activeSelf);
            }
        }

        foreach (var script in monoBehaviours)
        {
            if (script)
            {
                if (behaviour == EnableBehaviour.Enable)
                    script.enabled = true;
                else if (behaviour == EnableBehaviour.Disable)
                    script.enabled = false;
                else if (behaviour == EnableBehaviour.Toggle)
                    script.enabled = !script.enabled;
            }
        }

        yield return new WaitForSeconds(0.5f);

        robot.state = RoundRobot.State.Looking;

        activationSequence = null;
    }
}
