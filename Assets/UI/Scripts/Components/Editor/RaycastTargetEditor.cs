using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(RaycastTarget), false), CanEditMultipleObjects]
public class RaycastTargetEditor : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_Script);
        RaycastControlsGUI();
        serializedObject.ApplyModifiedProperties();
    }
}
