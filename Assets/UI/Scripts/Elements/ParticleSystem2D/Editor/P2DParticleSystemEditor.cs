using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.UI;

[CanEditMultipleObjects]
[CustomEditor(typeof(P2DParticleSystem), true)]
public class P2DParticleSystemEditor : GraphicEditor
{
    SerializedProperty emitProperty;
    SerializedProperty rateProperty;
    SerializedProperty maxParticlesProperty;
    SerializedProperty emitterTypeProperty;
    SerializedProperty particleSizeProperty;
    SerializedProperty spriteProperty;

    SerializedProperty lifeProperty;
    SerializedProperty lifeVariationProperty;

    SerializedProperty startSpeedProperty;
    SerializedProperty speedVariationProperty;
    SerializedProperty speedScaleProperty;

    SerializedProperty angleRelativeToVelocityProperty;
    SerializedProperty startAngleProperty;
    SerializedProperty angleVariationProperty;

    SerializedProperty startScaleProperty;
    SerializedProperty scaleVariationProperty;
    SerializedProperty scaleScaleProperty;

    SerializedProperty startAngularVelocityProperty;
    SerializedProperty angularVelocityVariationProperty;
    SerializedProperty angularVelocityScaleProperty;

    SerializedProperty startForceProperty;
    SerializedProperty forceVariationProperty;
    SerializedProperty forceScaleProperty;

    SerializedProperty startAlphaProperty;
    SerializedProperty alphaVariationProperty;
    SerializedProperty alphaScaleProperty;

    protected override void OnEnable()
    {
        base.OnEnable();

        emitProperty = serializedObject.FindProperty("emit");
        rateProperty = serializedObject.FindProperty("rate");
        maxParticlesProperty = serializedObject.FindProperty("maxParticles");
        emitterTypeProperty = serializedObject.FindProperty("emitterType");
        particleSizeProperty = serializedObject.FindProperty("particleSize");
        spriteProperty = serializedObject.FindProperty("sprite");
        
        lifeProperty = serializedObject.FindProperty("life");
        lifeVariationProperty = serializedObject.FindProperty("lifeVariation");
        
        startSpeedProperty = serializedObject.FindProperty("startSpeed");
        speedVariationProperty = serializedObject.FindProperty("speedVariation");
        speedScaleProperty = serializedObject.FindProperty("speedScale");

        angleRelativeToVelocityProperty = serializedObject.FindProperty("angleRelativeToVelocity");
        startAngleProperty = serializedObject.FindProperty("startAngle");
        angleVariationProperty = serializedObject.FindProperty("angleVariation");
        
        startScaleProperty = serializedObject.FindProperty("startScale");
        scaleVariationProperty = serializedObject.FindProperty("scaleVariation");
        scaleScaleProperty = serializedObject.FindProperty("scaleScale");
        
        startAngularVelocityProperty = serializedObject.FindProperty("startAngularVelocity");
        angularVelocityVariationProperty = serializedObject.FindProperty("angularVelocityVariation");
        angularVelocityScaleProperty = serializedObject.FindProperty("angularVelocityScale");
        
        startForceProperty = serializedObject.FindProperty("startForce");
        forceVariationProperty = serializedObject.FindProperty("forceVariation");
        forceScaleProperty = serializedObject.FindProperty("forceScale");

        startAlphaProperty = serializedObject.FindProperty("startAlpha");
        alphaVariationProperty = serializedObject.FindProperty("alphaVariation");
        alphaScaleProperty = serializedObject.FindProperty("alphaScale");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        
        EditorGUILayout.Space();

        if(spriteProperty.objectReferenceValue != null)
        {
            spriteProperty.objectReferenceValue = EditorGUILayout.ObjectField("Sprite", spriteProperty.objectReferenceValue, typeof(Sprite), allowSceneObjects: true);
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_Color);
        EditorGUILayout.PropertyField(m_Material);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Emission",  EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(emitProperty);
        EditorGUILayout.PropertyField(rateProperty);
        EditorGUILayout.PropertyField(maxParticlesProperty);
        EditorGUILayout.PropertyField(emitterTypeProperty);
        EditorGUILayout.PropertyField(particleSizeProperty);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Lifetime",  EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(lifeProperty);
        EditorGUILayout.PropertyField(lifeVariationProperty);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Speed",  EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(startSpeedProperty);
        EditorGUILayout.PropertyField(speedVariationProperty);
        EditorGUILayout.CurveField(speedScaleProperty, Color.green, new Rect(0, 0, 1, 1));
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scale",  EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(startScaleProperty);
        EditorGUILayout.PropertyField(scaleVariationProperty);
        EditorGUILayout.CurveField(scaleScaleProperty, Color.green, new Rect(0, 0, 1, 1));
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Angle",  EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(angleRelativeToVelocityProperty);
        EditorGUILayout.PropertyField(startAngleProperty);
        EditorGUILayout.PropertyField(angleVariationProperty);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Angular Velocity",  EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(startAngularVelocityProperty);
        EditorGUILayout.PropertyField(angularVelocityVariationProperty);
        EditorGUILayout.CurveField(angularVelocityScaleProperty, Color.green, new Rect(0, 0, 1, 1));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Force",  EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(startForceProperty);
        EditorGUILayout.PropertyField(forceVariationProperty);
        EditorGUILayout.CurveField(forceScaleProperty, Color.green, new Rect(0, 0, 1, 1));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Alpha",  EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(startAlphaProperty);
        EditorGUILayout.PropertyField(alphaVariationProperty);
        EditorGUILayout.CurveField(alphaScaleProperty, Color.green, new Rect(0, 0, 1, 1));

        serializedObject.ApplyModifiedProperties();
    }
}
