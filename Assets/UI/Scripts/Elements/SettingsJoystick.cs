using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;

public class SettingsJoystick : Selectable, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public CanvasScaler canvasScaler;
    public RectTransform rectTransform;
    public RectTransform joystickRing;
    public RectTransform joystickKnob;
    public RectTransform rangeBar;
    public TMP_Text scaleText;
    
    int pointerID = int.MinValue;

    public float Limit {
        get {
            return FromReferenceUnits(joystickKnob.anchoredPosition.x);
        }
        set {
            joystickKnob.anchoredPosition = new Vector2(ToReferenceUnits(value), 0);
            scaleText.text = value.ToString("0.0");
        }
    }

    protected override void Awake()
    {
        base.Awake();
        rangeBar.anchoredPosition = new Vector2(ToReferenceUnits(Defaults.minJoystickOffset), 0);
        rangeBar.sizeDelta = new Vector2(ToReferenceUnits(Defaults.maxJoystickOffset - Defaults.minJoystickOffset), 0);
    }

    float ToReferenceUnits(float cm) {
        return cm * Util.dpcm * (canvasScaler.referenceResolution.y / (float)Screen.height);
    }

    float FromReferenceUnits(float units) {
        return units * ((float)Screen.height / canvasScaler.referenceResolution.y) / Util.dpcm;
    }

    private void UpdateScaleText()
    {
        scaleText.text = FromReferenceUnits(joystickKnob.anchoredPosition.x).ToString("0.0");
    }

    float nextAdjustmentTime = 0;

    private void Update()
    {
        if(Time.time >= nextAdjustmentTime)
        {
            var dir = Gamepad.current.dpad.ReadValue();
            if (dir.x < -0.25)
            {
                Limit = Mathf.Clamp(Limit - 0.1f, Defaults.minJoystickOffset, Defaults.maxJoystickOffset);
                nextAdjustmentTime = Time.time + 0.1f;
            }
            else if(dir.x > 0.25f)
            {
                Limit = Mathf.Clamp(Limit + 0.1f, Defaults.minJoystickOffset, Defaults.maxJoystickOffset);
                nextAdjustmentTime = Time.time + 0.1f;
            }
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if(pointerID != int.MinValue)
            return;

        pointerID = eventData.pointerId;
        
        var pos = eventData.ScreenToLocalPoint(joystickRing);
        var posInCM = FromReferenceUnits(pos.x);
        posInCM = Util.Snap(posInCM, 0.1f);
        posInCM = Mathf.Clamp(posInCM, Defaults.minJoystickOffset, Defaults.maxJoystickOffset);
        joystickKnob.anchoredPosition = new Vector2(ToReferenceUnits(posInCM), 0);
        scaleText.text = posInCM.ToString("0.0");

        eventData.Use();

        base.OnPointerDown(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(pointerID == eventData.pointerId)
        {
            var pos = eventData.ScreenToLocalPoint(joystickRing);
            var posInCM = FromReferenceUnits(pos.x);
            posInCM = Util.Snap(posInCM, 0.1f);
            posInCM = Mathf.Clamp(posInCM, Defaults.minJoystickOffset, Defaults.maxJoystickOffset);
            joystickKnob.anchoredPosition = new Vector2(ToReferenceUnits(posInCM), 0);
            scaleText.text = posInCM.ToString("0.0");
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if(pointerID == eventData.pointerId)
        {
            pointerID = int.MinValue;
            // done
        }

        base.OnPointerUp(eventData);
    }
}
