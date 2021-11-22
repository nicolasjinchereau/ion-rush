using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(StackLayout)), CanEditMultipleObjects]
public class StackLayoutEditor : Editor
{
    SerializedProperty sizeToContentProperty;
    SerializedProperty minimumSizeProperty;
    SerializedProperty layoutAxisProperty;
    SerializedProperty alignmentProperty;
    SerializedProperty spacingProperty;
    SerializedProperty paddingProperty;

    private void OnEnable()
    {
        sizeToContentProperty = serializedObject.FindProperty("sizeToContent");
        minimumSizeProperty = serializedObject.FindProperty("minimumSize");
        layoutAxisProperty = serializedObject.FindProperty("layoutAxis");
        alignmentProperty = serializedObject.FindProperty("alignment");
        spacingProperty = serializedObject.FindProperty("spacing");
        paddingProperty = serializedObject.FindProperty("padding");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sizeToContentProperty);
        EditorGUILayout.PropertyField(minimumSizeProperty);
        EditorGUILayout.PropertyField(layoutAxisProperty);

        //if(!sizeToContentProperty.boolValue)
            EditorGUILayout.PropertyField(alignmentProperty);

        EditorGUILayout.PropertyField(spacingProperty);
        EditorGUILayout.PropertyField(paddingProperty, true);

        serializedObject.ApplyModifiedProperties();
    }
}
