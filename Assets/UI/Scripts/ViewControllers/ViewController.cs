using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewController : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    private RectTransform _rectTransform = null;
    
    public RectTransform rectTransform
    {
        get
        {
            if(!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();

            return _rectTransform;
        }
    }
}
