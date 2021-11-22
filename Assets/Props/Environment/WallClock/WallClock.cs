using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallClock : MonoBehaviour
{
    public Transform hourHand;
    public Transform minuteHand;
    public Transform secondHand;
    
    public int seconds = 0;

    public void Update()
    {
        int secs = seconds % 60;
        int totalMins = (seconds - secs) / 60;
        int mins = totalMins % 60;
        int totalHours = (totalMins - mins) / 60;
        int hours = totalHours % 24;
        
        hourHand.localRotation = Quaternion.Euler(0, -hours * 30 - mins / 2, 0);
        minuteHand.localRotation = Quaternion.Euler(0, -mins * 6, 0);
        secondHand.localRotation = Quaternion.Euler(0, -secs * 6, 0);
    }

    public void Set(int hours, int mins, int seconds)
    {
        this.seconds = hours * 3600 + mins * 60 + seconds;
    }
}
