using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum MouseButton
{
    LeftButton,
    MiddleButton,
    RightButton,
    BackButton,
    ForwardButton
}

public static class MouseExtensions
{
    private static ButtonControl GetButton(Mouse mouse, MouseButton button)
    {
        switch(button)
        {
            default:
            case MouseButton.LeftButton: return mouse.leftButton;
            case MouseButton.MiddleButton: return mouse.middleButton;
            case MouseButton.RightButton: return mouse.rightButton;
            case MouseButton.BackButton: return mouse.backButton;
            case MouseButton.ForwardButton: return mouse.forwardButton;
        }
    }

    public static bool WasPressedThisFrame(this Mouse mouse, MouseButton button) {
        return mouse != null ? GetButton(mouse, button).wasPressedThisFrame : false;
    }

    public static bool IsPressed(this Mouse mouse, MouseButton button) {
        return mouse != null ? GetButton(mouse, button).isPressed : false;
    }

    public static bool WasReleasedThisFrame(this Mouse mouse, MouseButton button) {
        return mouse != null ? GetButton(mouse, button).wasReleasedThisFrame : false;
    }
}
