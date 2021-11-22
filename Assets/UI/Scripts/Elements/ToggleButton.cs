using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

[Serializable]
public class ToggleButton : Button
{
    [Serializable]
    public class ToggleEvent : UnityEvent<ToggleButton>
    {
    }

    public Image imageTarget;
    public Sprite offImage;
    public Sprite onImage;

    public TMPro.TMP_Text onText;
    public TMPro.TMP_Text offText;

    public GameObject onContent;
    public GameObject offContent;

    public Graphic[] backgroundTargets;
    public Graphic[] foregroundTargets;
    public Color backgroundOffColor = Color.white;
    public Color backgroundOnColor = Color.white;
    public Color foregroundOffColor = Color.white;
    public Color foregroundOnColor = Color.white;
    public ToggleEvent onToggle;

    [SerializeField] bool state = false;
    [SerializeField] bool toggleOnPress = true;

    public bool State {
        get { return state; }
        set { SetState(value); }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (toggleOnPress)
            SetState(!state);

        base.OnPointerClick(eventData);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        if (toggleOnPress)
            SetState(!state);

        base.OnSubmit(eventData);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateGraphics();
    }

    public void SetState(bool newState, bool invokeCallback = true)
    {
        state = newState;
        UpdateGraphics();
        if(invokeCallback) onToggle?.Invoke(this);
    }

    public void UpdateGraphics()
    {
        if(imageTarget)
        {
            //#if UNITY_EDITOR
            //UnityEditor.Undo.RecordObject(imageTarget, "Set ToggleButton State");
            //#endif

            var sprite = state ? onImage : offImage;

            imageTarget.sprite = sprite;

            var ss = spriteState;
            ss.highlightedSprite = sprite;
            ss.disabledSprite = sprite;
            spriteState = ss;
        }

        if (onText && offText)
        {
            onText.gameObject.SetActive(state);
            offText.gameObject.SetActive(!state);
        }

        if (onContent && offContent)
        {
            onContent.gameObject.SetActive(state);
            offContent.gameObject.SetActive(!state);
        }

        if (backgroundTargets != null)
        {
            foreach(var target in backgroundTargets)
            {
                if(!target)
                    continue;
                
                //#if UNITY_EDITOR
                //UnityEditor.Undo.RecordObject(target, "Set ToggleButton State");
                //#endif

                target.color = state ? backgroundOnColor : backgroundOffColor;
            }
        }

        if(foregroundTargets != null)
        {
            foreach(var target in foregroundTargets)
            {
                if(!target)
                    continue;

                //#if UNITY_EDITOR
                //UnityEditor.Undo.RecordObject(target, "Set ToggleButton State");
                //#endif

                target.color = state ? foregroundOnColor : foregroundOffColor;
            }
        }
    }
}
