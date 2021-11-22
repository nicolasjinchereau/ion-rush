using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(SolidPlane), true)]
[CanEditMultipleObjects]
public class SolidPlaneEditor : GraphicEditor
{
    protected override void OnEnable()
    {
        base.OnEnable();
        base.SetShowNativeSize(false, true);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        AppearanceControlsGUI();
        RaycastControlsGUI();
        base.SetShowNativeSize(false, false);

        serializedObject.ApplyModifiedProperties();
    }

    public override bool HasPreviewGUI() {
        return true;
    }

    public override void OnPreviewGUI(Rect rect, GUIStyle background)
    {
        SolidPlane solidPlane = target as SolidPlane;
        EditorGUILayout.ColorField(Color.white);
    }

    public override string GetInfoString()
    {
        SolidPlane solidPlane = target as SolidPlane;
        return string.Format("SolidPlane");
    }
}
