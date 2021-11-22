using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BonusGearCounter : GearCounter
{
    public RectTransform rectTransform;

    public Image gear;
    public TMP_Text count;
    public P2DParticleSystem particles;

    int _gearCount = 0;
    int _gearQuota = 300;

    public override int gearCount
    {
        get { return _gearCount; }
        set {
            _gearCount = value;
            UpdateCount();
        }
    }

    public override int gearQuota
    {
        get { return _gearQuota; }
        set
        {
            _gearQuota = value;
            UpdateCount();
        }
    }

    void UpdateCount() {
        count.text = _gearCount.ToString() + "/" + _gearQuota.ToString();
    }

    public override void AddGear(Vector3 fromWorldPosition) {
        StartCoroutine(_addGear(fromWorldPosition));
    }

    IEnumerator _addGear(Vector3 fromWorldPosition)
    {
        yield return null;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(fromWorldPosition);
        Vector2 uiPos = Vector2.zero;

        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, null, out uiPos))
            yield break;

        var gearIcon = gear;

        Vector2 start = uiPos;
        Vector2 finish = gearIcon.rectTransform.anchoredPosition;

        Image gearImg = Instantiate(gearIcon.gameObject, rectTransform).GetComponent<Image>();
        gearImg.SetActive(true);

        float length = 0.3f;

        yield return StartCoroutine(Util.Blend(length, t => {
            t = Curve.SmoothStepInSteep(t);
            gearImg.rectTransform.anchoredPosition = Vector2.Lerp(start, finish, t);
        }));

        ++this.gearCount;

        Destroy(gearImg.gameObject);
        particles.Emit(20);
    }
}
