using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UI;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(ToggleButton), true)]
public class ToggleButtonEditor : ButtonEditor
{
    SerializedProperty imageTargetProperty;
    SerializedProperty offSpriteProperty;
    SerializedProperty onSpriteProperty;

    SerializedProperty offTextProperty;
    SerializedProperty onTextProperty;

    SerializedProperty offContentProperty;
    SerializedProperty onContentProperty;

    SerializedProperty backgroundTargetsProperty;
    SerializedProperty foregroundTargetsProperty;
    SerializedProperty backgroundOffColorProperty;
    SerializedProperty backgroundOnColorProperty;
    SerializedProperty foregroundOffColorProperty;
    SerializedProperty foregroundOnColorProperty;

    SerializedProperty stateProperty;
    SerializedProperty toggleOnPressProperty;
    SerializedProperty onToggleProperty;

    protected override void OnEnable()
    {
        base.OnEnable();

        imageTargetProperty = serializedObject.FindProperty("imageTarget");
        
        offSpriteProperty = serializedObject.FindProperty("offImage");
        onSpriteProperty = serializedObject.FindProperty("onImage");
        offTextProperty = serializedObject.FindProperty("offText");
        onTextProperty = serializedObject.FindProperty("onText");
        offContentProperty = serializedObject.FindProperty("offContent");
        onContentProperty = serializedObject.FindProperty("onContent");

        backgroundTargetsProperty = serializedObject.FindProperty("backgroundTargets");
        foregroundTargetsProperty = serializedObject.FindProperty("foregroundTargets");
        backgroundOffColorProperty = serializedObject.FindProperty("backgroundOffColor");
        backgroundOnColorProperty = serializedObject.FindProperty("backgroundOnColor");
        foregroundOffColorProperty = serializedObject.FindProperty("foregroundOffColor");
        foregroundOnColorProperty = serializedObject.FindProperty("foregroundOnColor");
        
        stateProperty = serializedObject.FindProperty("state");
        toggleOnPressProperty = serializedObject.FindProperty("toggleOnPress");
        onToggleProperty = serializedObject.FindProperty("onToggle");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(imageTargetProperty);
        
        EditorGUILayout.PropertyField(offSpriteProperty);
        EditorGUILayout.PropertyField(onSpriteProperty);
        EditorGUILayout.PropertyField(offTextProperty);
        EditorGUILayout.PropertyField(onTextProperty);
        EditorGUILayout.PropertyField(offContentProperty);
        EditorGUILayout.PropertyField(onContentProperty);

        EditorGUILayout.PropertyField(backgroundTargetsProperty, true);
        EditorGUILayout.PropertyField(foregroundTargetsProperty, true);
        EditorGUILayout.PropertyField(backgroundOffColorProperty);
        EditorGUILayout.PropertyField(backgroundOnColorProperty);
        EditorGUILayout.PropertyField(foregroundOffColorProperty);
        EditorGUILayout.PropertyField(foregroundOnColorProperty);

        EditorGUILayout.PropertyField(stateProperty);
        EditorGUILayout.PropertyField(toggleOnPressProperty);

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.PropertyField(onToggleProperty);
        serializedObject.ApplyModifiedProperties();

        if(GUI.changed)
            (target as ToggleButton).UpdateGraphics();
    }
}
