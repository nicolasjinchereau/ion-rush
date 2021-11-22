using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicGearCounter : GearCounter
{
    public RectTransform rectTransform;

    public Image[] uncollectedGears;
    public Image[] collectedGears;
    public P2DParticleSystem[] gearParticles;

    private int _gearCount = 0;

    public override int gearCount
    {
        set
        {
            _gearCount = value;

            for(int i = 0; i < 3; ++i)
            {
                bool collected = _gearCount > i;
                uncollectedGears[i].gameObject.SetActive(!collected);
                collectedGears[i].gameObject.SetActive(collected);
            }
        }

        get { return _gearCount; }
    }

    public override int gearQuota
    {
        get { return 3; }
        set { }
    }

    public override void AddGear(Vector3 fromWorldPosition)
    {
        StartCoroutine(_addGear(fromWorldPosition));
    }

    IEnumerator _addGear(Vector3 fromWorldPosition)
    {
        yield return null;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(fromWorldPosition);
        Vector2 uiPos = Vector2.zero;

        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, null, out uiPos))
            yield break;

        var gearIcon = collectedGears[_gearCount];

        Vector2 start = uiPos;
        Vector2 finish = gearIcon.rectTransform.anchoredPosition;

        Image gearImg = Instantiate(gearIcon.gameObject, rectTransform).GetComponent<Image>();
        gearImg.SetActive(true);

        float length = 0.25f;

        yield return StartCoroutine(Util.Blend(length, t => {
            t = Curve.SmoothStepInSteep(t);
            gearImg.rectTransform.anchoredPosition = Vector2.Lerp(start, finish, t);
        }));

        Destroy(gearImg.gameObject);
        gearParticles[gearCount].Emit(20);
        
        ++gearCount;
    }
}
