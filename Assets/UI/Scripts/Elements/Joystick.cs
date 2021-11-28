using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public bool isStatic = false;
    public Image ring;
    public Image knob;

    Vector2 downPos;
    int pointerID = int.MinValue;
    float maxRadius = 100.0f;
    CanvasScaler canvasScaler;

    public Vector3 direction {
        get; set;
    }

    public RectTransform rectTransform {
        get { return transform as RectTransform; }
    }

    public float CentimetersToCanvasScale {
        get { return Util.dpcm * (canvasScaler.referenceResolution.y / (float)Camera.main.pixelHeight); }
    }

    public bool IsDown {
        get => pointerID != int.MinValue;
    }

    public virtual GamepadControl StickControl => GamepadControl.LeftStick;
    public virtual Key LeftDirKey => Key.A;
    public virtual Key RightDirKey => Key.D;
    public virtual Key ForwardDirKey => Key.W;
    public virtual Key BackwardDirKey => Key.S;

    protected virtual void OnEnable()
    {
        ring.SetActive(isStatic);
        canvasScaler = GetComponentInParent<CanvasScaler>();
        maxRadius = CentimetersToCanvasScale * Profile.JoystickLimit;
    }

    protected virtual void OnDisable()
    {
        bool updateDirection = pointerID != int.MinValue;

        pointerID = int.MinValue;
        direction = Vector3.zero;
        knob.rectTransform.anchoredPosition = Vector2.zero;
        ring.SetActive(isStatic);

        if(updateDirection)
            OnUpdateDirection(Vector2.zero);
    }

    protected virtual void OnUpdateDirection(Vector3 direction) {}

    void MoveKnob(Vector2 pos)
    {
        Vector2 offset = pos - ring.rectTransform.anchoredPosition;
        float length = offset.magnitude;

        Vector2 dir;

        if (length > maxRadius)
        {
            dir = offset / length;
            offset = dir * maxRadius;
        }
        else
        {
            dir = offset / maxRadius;
        }

        direction = new Vector3(dir.x, 0, dir.y);
        knob.rectTransform.anchoredPosition = offset;
        OnUpdateDirection(direction);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(pointerID == int.MinValue)
        {
            pointerID = eventData.pointerId;
            knob.rectTransform.anchoredPosition = Vector2.zero;
            
            if(!isStatic)
            {
                var pos = eventData.ScreenToLocalPoint(rectTransform);
                ring.rectTransform.anchoredPosition = pos;
                ring.SetActive(true);
            }
            
            OnUpdateDirection(Vector2.zero);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(pointerID == eventData.pointerId)
        {
            var pos = eventData.ScreenToLocalPoint(rectTransform);
            MoveKnob(pos);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(pointerID == eventData.pointerId)
        {
            pointerID = int.MinValue;
            knob.rectTransform.anchoredPosition = Vector2.zero;
            direction = Vector3.zero;
            
            if(!isStatic)
                ring.SetActive(false);

            OnUpdateDirection(Vector2.zero);
        }
    }
    
    void Update()
    {
        if (!IsDown)
        {
            bool l = Keyboard.current.IsPressed(LeftDirKey);
            bool r = Keyboard.current.IsPressed(RightDirKey);
            bool f = Keyboard.current.IsPressed(ForwardDirKey);
            bool b = Keyboard.current.IsPressed(BackwardDirKey);
            Vector2 stick = Gamepad.current.GetDirection(StickControl);

            Vector3 dir = Vector3.zero;

            if (l || r || f || b)
            {
                if (l) dir.x -= 1.0f;
                if (r) dir.x += 1.0f;
                if (b) dir.z -= 1.0f;
                if (f) dir.z += 1.0f;
                dir.Normalize();
            }
            else if(stick.magnitude > 0.001f)
            {
                dir = new Vector3(stick.x, 0, stick.y);
            }

            OnUpdateDirection(dir);
        }
    }
}
