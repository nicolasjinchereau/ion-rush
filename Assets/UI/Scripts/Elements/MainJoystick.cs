using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MainJoystick : Joystick
{
    protected override void OnDisable()
    {
        base.OnDisable();

        if(Player.exists)
            Player.direction = Vector3.zero;
    }

    protected override void OnUpdateDirection(Vector3 direction)
    {
        if (Player.exists)
            Player.direction = direction;
    }
}
