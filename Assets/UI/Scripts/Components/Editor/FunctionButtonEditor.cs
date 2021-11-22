using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomEditor(typeof(FunctionButton))]
public class FunctionButtonEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var tar = target as FunctionButton;

        if (tar && tar.target)
        {
            var methods = tar.target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                if (method.GetParameters().Length != 0)
                    continue;

                if (GUILayout.Button(method.Name, EditorStyles.miniButton))
                {
                    if (Application.isPlaying)
                    {
                        method.Invoke(tar.target, new object[0]);
                    }
                    else
                    {
                        Debug.LogError("FunctionButton only works while playing.");
                    }
                }
            }
        }
    }
}
