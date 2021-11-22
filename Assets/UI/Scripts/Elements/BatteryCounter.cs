using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BatteryCounter : MonoBehaviour
{
    public RectTransform rectTransform;
    
    public Image batteryIcon;
    public TMP_Text countLabel;
    public P2DParticleSystem particles;

    private int _batteryCount = 0;
    public int batteryCount
    {
        get { return _batteryCount; }
        set
        {
            if(value != _batteryCount)
            {
                _batteryCount = value;
                countLabel.text = _batteryCount.ToString();
            }
        }
    }

    public void AddBatteries(Vector3 fromWorldPosition, int value) {
        StartCoroutine(_addBatteries(fromWorldPosition, value));
    }

    IEnumerator _addBatteries(Vector3 fromWorldPosition, int value)
    {
        yield return null;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(fromWorldPosition);
        Vector2 uiPos = Vector2.zero;

        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, null, out uiPos))
            yield break;

        Vector2 start = uiPos;
        Vector2 finish = batteryIcon.rectTransform.anchoredPosition;

        Image batImg = Instantiate(batteryIcon.gameObject, rectTransform).GetComponent<Image>();

        float length = 0.2f;
        
        yield return StartCoroutine(Util.Blend(length, t => {
            t = Curve.SmoothStepInSteep(t);
            batImg.rectTransform.anchoredPosition = Vector2.Lerp(start, finish, t);
        }));

        this.batteryCount += value;

        Destroy(batImg.gameObject);
        particles.Emit(20);
    }
}
