using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class JumpArea : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if(Player.that)
            Player.that.DoJump();
    }

    private void Update()
    {
        if (Keyboard.current.WasPressedThisFrame(Key.Space) ||
            Gamepad.current.WasPressedThisFrame(GamepadButton.A))
        {
            Player.that.DoJump();
        }
    }
}
