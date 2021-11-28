using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class KeyboardExtensions
{
    public static bool WasPressedThisFrame(this Keyboard keyboard, Key key)  {
        return keyboard != null ? keyboard[key].wasPressedThisFrame : false;
    }

    public static bool IsPressed(this Keyboard keyboard, Key key) {
        return keyboard != null ? keyboard[key].isPressed : false;
    }

    public static bool WasReleasedThisFrame(this Keyboard keyboard, Key key) {
        return keyboard != null ? keyboard[key].wasReleasedThisFrame : false;
    }
}
