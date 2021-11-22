using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public enum StackLayoutAxis
{
    Horizontal,
    Vertical
}

[Serializable]
public enum StackLayoutAlignment
{
    Start,
    Middle,
    End
}

[ExecuteInEditMode]
public class StackLayout : UIBehaviour, ILayoutGroup, ILayoutSelfController
{
    public bool sizeToContent = false;
    public float minimumSize = 0;
    public StackLayoutAxis layoutAxis = StackLayoutAxis.Vertical;
    public StackLayoutAlignment alignment = StackLayoutAlignment.Middle;
    public float spacing = 0;
    public RectOffset padding = new RectOffset();

    public RectTransform rectTransform {
        get { return transform as RectTransform; }
    }

    public void SetLayoutVertical()
    {
        if(layoutAxis == StackLayoutAxis.Vertical)
            UpdateLayout();
    }

    public void SetLayoutHorizontal()
    {
        if(layoutAxis == StackLayoutAxis.Horizontal)
            UpdateLayout();
    }

    float GetStartPadding(StackLayoutAxis axis)
    {
        if(axis == StackLayoutAxis.Horizontal)
            return padding.left;
        else if(axis == StackLayoutAxis.Vertical)
            return padding.top;
        else
            throw new Exception("Invalid StackLayoutAxis");
    }

    float GetEndPadding(StackLayoutAxis axis)
    {
        if(axis == StackLayoutAxis.Horizontal)
            return padding.right;
        else if(axis == StackLayoutAxis.Vertical)
            return padding.bottom;
        else
            throw new Exception("Invalid StackLayoutAxis");
    }

    float GetTotalPadding(StackLayoutAxis axis)
    {
        if(axis == StackLayoutAxis.Horizontal)
            return padding.left + padding.right;
        else if(axis == StackLayoutAxis.Vertical)
            return padding.top + padding.bottom;
        else
            throw new Exception("Invalid StackLayoutAxis");
    }

    public void UpdateLayout()
    {
        int axisIndex = (int)layoutAxis;
        int otherAxisIndex = axisIndex == 0 ? 1 : 0;

        float contentSize = 0;
        float contentMax = 0;
        int activeChildren = 0;

        for(int i = 0; i < rectTransform.childCount; ++i)
        {
            var child = rectTransform.GetChild(i) as RectTransform;
            if(child == null || !child.gameObject.activeInHierarchy)
                continue;

            ++activeChildren;

            contentSize += child.rect.size[axisIndex];
            contentMax = Mathf.Max(contentMax, child.rect.size[otherAxisIndex]);
        }

        if(activeChildren > 1)
            contentSize += (activeChildren - 1) * spacing;
        
        float mySize = 0;
        float myMax = 0;

        if(sizeToContent)
        {
            #if UNITY_EDITOR
            Undo.RecordObject(rectTransform, "Stack Layout");
            #endif

            mySize = Mathf.Max(contentSize, minimumSize) + GetTotalPadding(layoutAxis);
            myMax = contentMax + GetTotalPadding((StackLayoutAxis)otherAxisIndex);

            var size = Vector2.zero;
            size[axisIndex] = mySize;
            size[otherAxisIndex] = myMax;
            rectTransform.sizeDelta = size;
        }
        else
        {
            mySize = rectTransform.rect.size[axisIndex];
            myMax = rectTransform.rect.size[otherAxisIndex];
        }

        float start = 0;

        if(layoutAxis == StackLayoutAxis.Horizontal)
        {
            if(alignment == StackLayoutAlignment.Start)
                start = GetStartPadding(layoutAxis);
            else if(alignment == StackLayoutAlignment.Middle)
                start = (mySize - contentSize) / 2.0f;
            else if(alignment == StackLayoutAlignment.End)
                start = (mySize - contentSize) - GetEndPadding(layoutAxis);
        }
        else if(layoutAxis == StackLayoutAxis.Vertical)
        {
            if(alignment == StackLayoutAlignment.Start)
                start = -GetStartPadding(layoutAxis);
            else if(alignment == StackLayoutAlignment.Middle)
                start = -(mySize - contentSize) / 2.0f;
            else if(alignment == StackLayoutAlignment.End)
                start = -(mySize - contentSize) + GetEndPadding(layoutAxis);
        }

        float otherAxisStart = 0;

        if(sizeToContent)
        {
            if(layoutAxis == StackLayoutAxis.Horizontal) {
                otherAxisStart = myMax * 0.5f - contentMax * 0.5f - GetStartPadding((StackLayoutAxis)otherAxisIndex);
            }
            else if(layoutAxis == StackLayoutAxis.Vertical) {
                otherAxisStart = -myMax * 0.5f + contentMax * 0.5f + GetStartPadding((StackLayoutAxis)otherAxisIndex);
            }
        }

        for(int i = 0, j = 0; i < rectTransform.childCount; ++i)
        {
            var child = rectTransform.GetChild(i) as RectTransform;
            if(child == null || !child.gameObject.activeInHierarchy)
                continue;
            
#if UNITY_EDITOR
            Undo.RecordObject(child, "Stack Layout");
#endif
            var pivotOffset = Vector2.Scale(child.pivot - new Vector2(0.5f, 0.5f), child.rect.size);

            if (layoutAxis == StackLayoutAxis.Horizontal)
            {
                if(j > 0)
                    start += spacing;

                child.anchorMin = new Vector2(0, 0.5f);
                child.anchorMax = new Vector2(0, 0.5f);
                child.anchoredPosition = pivotOffset + new Vector2(start + child.rect.size[axisIndex] / 2, otherAxisStart);
                start += child.rect.size[axisIndex];
            }
            else if(layoutAxis == StackLayoutAxis.Vertical)
            {
                if(j > 0)
                    start -= spacing;
                
                child.anchorMin = new Vector2(0.5f, 1.0f);
                child.anchorMax = new Vector2(0.5f, 1.0f);
                child.anchoredPosition = pivotOffset + new Vector2(otherAxisStart, start - child.rect.size[axisIndex] / 2);
                start -= child.rect.size[axisIndex];
            }

            ++j;
        }
    }

    private void Update()
    {
        bool dirty = false;

        for(int i = 0; i < rectTransform.childCount; ++i)
        {
            var child = rectTransform.GetChild(i);
            if(child == null || !child.gameObject.activeInHierarchy)
                continue;

            if(child.hasChanged) {
                dirty = true;
                child.hasChanged = false;
            }
        }

        if(dirty)
            SetDirty();
    }

    protected override void OnEnable() {
        SetDirty();
    }

    protected override void OnDisable() {
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    protected override void OnRectTransformDimensionsChange() {
        SetDirty();
    }

    protected void OnTransformChildrenChanged() {
        SetDirty();
    }

    protected void SetDirty()
    {
        if(!IsActive())
            return;

        if(!CanvasUpdateRegistry.IsRebuildingLayout())
        {
            #if UNITY_EDITOR
            Undo.RecordObject(rectTransform, "Stack Layout");
            #endif

            //LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            //#else
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            //#endif
        }
        else
        {
            StartCoroutine(DelayedSetDirty(rectTransform));
        }
    }

    IEnumerator DelayedSetDirty(RectTransform rt)
    {
        yield return null;

        #if UNITY_EDITOR
        Undo.RecordObject(rectTransform, "Stack Layout");
        #endif
        //LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        //#else
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        //#endif
    }

    #if UNITY_EDITOR
    protected override void OnValidate() {
        SetDirty();
    }
    #endif
}
