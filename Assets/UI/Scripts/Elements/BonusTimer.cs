using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BonusTimer : MonoBehaviour
{
    public TMP_Text timerText;
    
    private int _seconds = 0;
    public int seconds
    {
        get { return _seconds; }
        set
        {
            if (_seconds != value)
            {
                _seconds = value;

                int remainingTime = Mathf.Max(_seconds, 0);

                int cmins = remainingTime / 60;
                int csecs = remainingTime % 60;

                timerText.text = $"{cmins}:{csecs:00}";
            }
        }
    }
}
