using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;

public class Useable : MonoBehaviour
{
    public enum InputType
    {
        None,
        Button,
        Joystick,
        ButtonAndJoystick,
    }

    public InputType inputType = InputType.Button;
    public bool unused = true;
    public bool flashEnabled = false;
    public bool ready = true;

    private float _highlightTime = 0.0f;
    private bool _isFlashing = false;

    // return 1 if action succeeded, 0 if not (to play error sound) or -1 if not implemented
    public virtual int OnAction(){ return -1; }
    public virtual void OnUpdateDirection(Vector3 direction) {}
    public virtual void OnUseStart(){}
    public virtual void OnUseFinish(){}

    private List<Material> targetMaterials = new List<Material>();

    public void Awake()
    {
        OnAwake();
    }

    // base.OnAwake() must always be called when overriding this method
    public virtual void OnAwake()
    {
        MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>();

        for(int i = 0; i < renderers.Length; ++i)
        {
            if(renderers[i].sharedMaterial.HasProperty("_Highlight"))
            {
                Material mtl = renderers[i].material;
                mtl.SetFloat("_Highlight", 0.0f);
                targetMaterials.Add(mtl);
            }
        }
    }

    public void PerformAction()
    {
        int ret = OnAction();
        if (ret == 1)
            SharedSounds.useButton.Play();
        else if (ret == 0)
            SharedSounds.error.Play();

        if (unused)
        {
            flashEnabled = false;
            unused = false;
        }
    }

    public void UpdateDirection(Vector3 direction)
    {
        OnUpdateDirection(direction);

        if (unused && direction.magnitude > 0.001f)
        {
            flashEnabled = false;
            unused = false;
        }
    }

    public void StartUse()
    {
        if (unused)
            flashEnabled = true;

        OnUseStart();
    }

    public void FinishUse()
    {
        flashEnabled = false;
        OnUseFinish();
    }

    public void LateUpdate()
    {
        if (flashEnabled) {
            _isFlashing = true;
        }

        if (_isFlashing)
        {
            _highlightTime += Time.deltaTime;

            if (_highlightTime >= 1.0f)
            {
                if (flashEnabled)
                {
                    _highlightTime -= 1.0f;
                }
                else
                {
                    _highlightTime = 0.0f;
                    _isFlashing = false;
                }
            }

            float highlight = (Mathf.Cos(_highlightTime * (Mathf.PI * 2.0f)) - 1.0f) * -0.5f;

            foreach (Material mtl in targetMaterials)
                mtl.SetFloat("_Highlight", highlight);
        }
    }
}
