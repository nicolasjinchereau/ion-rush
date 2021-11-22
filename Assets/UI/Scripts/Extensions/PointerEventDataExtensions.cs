using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class PointerEventDataExtensions
{
    public static Vector2 ScreenToLocalPoint(this PointerEventData eventData, RectTransform rectTransform)
    {
        Vector2 localPoint = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPoint);
        return localPoint;
    }
}
