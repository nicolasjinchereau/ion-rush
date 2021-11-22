using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class JumpArea : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if(Player.that)
            Player.that.DoJump();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame ||
            Gamepad.current.aButton.wasPressedThisFrame)
        {
            Player.that.DoJump();
        }
    }
}
