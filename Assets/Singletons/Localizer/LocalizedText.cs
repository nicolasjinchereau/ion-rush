using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[Serializable]
public class LocalizedTextChangedEvent : UnityEvent {

}

[ExecuteInEditMode]
public class LocalizedText : MonoBehaviour
{
    public string key;
    public LocalizedTextChangedEvent onTextChanged;

    Text textElement;

    private void Awake()
    {
        textElement = GetComponent<Text>();
    }

    private void OnEnable()
    {
        if(!textElement)
            textElement = GetComponent<Text>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateText();
    }
#endif

    private void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if(textElement != null && !string.IsNullOrEmpty(key))
        {
            var text = Localizer.Get(key);
            if(text != null)
            {
                #if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(textElement, "Update LocalizedText Text");
                #endif

                textElement.text = text;

                #if UNITY_EDITOR
                textElement.SetAllDirty();
                #endif

                onTextChanged.Invoke();
            }
        }
	}
}
