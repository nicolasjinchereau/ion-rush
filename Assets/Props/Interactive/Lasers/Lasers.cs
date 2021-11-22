using UnityEngine;
using System.Collections;

public class Lasers : Useable
{
    public GameObject[] beams;
    public Collider laserTrigger;

    private bool _lasersEnabled = true;

    public override void OnAwake() {
        base.OnAwake();
        SetLasersEnabled(beams[0].activeSelf, false);
    }

    public bool lasersEnabled {
        get { return _lasersEnabled; }
        set { SetLasersEnabled(value, true); }
    }

    public void SetLasersEnabled(bool enabled, bool playSound = true)
    {
        if(_lasersEnabled != enabled)
        {
            _lasersEnabled = enabled;

            if(playSound)
            {
                if(_lasersEnabled)
                    SharedSounds.lasersOn.Play();
                else
                    SharedSounds.lasersOff.Play();
            }
            
            foreach(GameObject go in beams)
                go.SetActive(_lasersEnabled);

            laserTrigger.enabled = _lasersEnabled;
        }
    }

    public override void OnUseStart()
    {
        
    }

    public override void OnUseFinish()
    {
        
    }

    public override int OnAction() {
        lasersEnabled = !lasersEnabled;
        return 1;
    }
}
