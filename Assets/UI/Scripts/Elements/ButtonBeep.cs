using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBeep : MonoBehaviour
{
    public void OnPress() {
        SharedSounds.button.Play();
    }
}
