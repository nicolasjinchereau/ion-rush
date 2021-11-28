using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public enum GamepadControl
{
    LeftStick,
    RightStick,
    DPad
}

public static class GamepadExtensions
{
    public static bool WasPressedThisFrame(this Gamepad gamepad, GamepadButton button) {
        return gamepad != null ? gamepad[button].wasPressedThisFrame : false;
    }

    public static bool IsPressed(this Gamepad gamepad, GamepadButton button) {
        return gamepad != null ? gamepad[button].isPressed : false;
    }

    public static bool WasReleasedThisFrame(this Gamepad gamepad, GamepadButton button) {
        return gamepad != null ? gamepad[button].wasReleasedThisFrame : false;
    }

    public static Vector2 GetDirection(this Gamepad gamepad, GamepadControl control)
    {
        Vector2 dir = Vector2.zero;

        if (gamepad != null)
        {
            if (control == GamepadControl.LeftStick && gamepad.leftStick != null)
                dir = gamepad.leftStick.ReadValue();
            else if (control == GamepadControl.RightStick && gamepad.rightStick != null)
                dir = gamepad.rightStick.ReadValue();
            else if (control == GamepadControl.DPad && gamepad.dpad != null)
                dir = gamepad.dpad.ReadValue();
        }

        return dir;
    }
}
