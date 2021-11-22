using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UsePanel : MonoBehaviour
{
    private const float ControlMargin = 200;

    public UseButton useButton;
    public UseJoystick useJoystick;

    private Useable _target = null;

    public Useable target
    {
        set
        {
            if (_target == value)
                return;

            if(_target) target.FinishUse();
            _target = value;

            if(_target)
            {
                if(_target.inputType == Useable.InputType.Button)
                {
                    useButton.SetActive(true);
                    useButton.target = _target;
                    useButton.button.interactable = _target.ready;
                    useButton.buttonImage.color = _target.ready ? Color.white : Color.gray;
                }
                else if (_target.inputType == Useable.InputType.Joystick)
                {
                    useJoystick.SetActive(true);
                    useJoystick.target = _target;
                }
                else if(_target.inputType == Useable.InputType.ButtonAndJoystick)
                {
                    useJoystick.SetActive(true);
                    useJoystick.target = _target;

                    useButton.SetActive(true);
                    useButton.target = _target;
                    useButton.button.interactable = _target.ready;
                    useButton.buttonImage.color = _target.ready ? Color.white : Color.gray;
                }

                _target.StartUse();
            }
            else
            {
                useButton.target = null;
                useButton.SetActive(false);
                useJoystick.target = null;
                useJoystick.SetActive(false);
            }
        }

        get {
            return _target;
        }
    }

    private void Update()
    {
        if (_target &&
            (_target.inputType == Useable.InputType.Button || _target.inputType == Useable.InputType.ButtonAndJoystick))
        {
            useButton.button.interactable = _target.ready;
            useButton.buttonImage.color = _target.ready ? Color.white : Color.gray;
        }
    }
}
