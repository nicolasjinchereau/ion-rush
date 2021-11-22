using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinCounter : MonoBehaviour
{
    public RectTransform rectTransform;

    public Image coinIcon;
    public TMP_Text countLabel;
    public P2DParticleSystem particles;

    private int _coinCount = 0;
    public int coinCount
    {
        get { return _coinCount; }
        set
        {
            if(value != _coinCount)
            {
                _coinCount = value;
                countLabel.text = _coinCount.ToString();
            }
        }
    }

    public void AddCoins(Vector3 fromWorldPosition, int value) {
        StartCoroutine(_addCoins(fromWorldPosition, value));
    }

    IEnumerator _addCoins(Vector3 fromWorldPosition, int value)
    {
        yield return null;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(fromWorldPosition);
        Vector2 uiPos = Vector2.zero;

        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, null, out uiPos))
            yield break;

        Vector2 start = uiPos;
        Vector2 finish = coinIcon.rectTransform.anchoredPosition;

        Image coinImg = Instantiate(coinIcon.gameObject, rectTransform).GetComponent<Image>();

        float length = 0.25f;

        yield return StartCoroutine(Util.Blend(length, t => {
            t = Curve.SmoothStepInSteep(t);
            coinImg.rectTransform.anchoredPosition = Vector2.Lerp(start, finish, t);
        }));

        this.coinCount += value;

        Destroy(coinImg.gameObject);
        particles.Emit(20);
    }
}
