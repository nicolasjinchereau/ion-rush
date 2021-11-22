using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuantumComputer : MonoBehaviour
{
    public MeshRenderer topButtonsRenderer;
    public MeshRenderer bottomButtonsRenderer;

    private void Awake()
    {
        var topBlock = new MaterialPropertyBlock();
        var botBlock = new MaterialPropertyBlock();
        topBlock.SetFloat("_TimeOffset", Random.value);
        botBlock.SetFloat("_TimeOffset", Random.value);
        topButtonsRenderer.SetPropertyBlock(topBlock);
        bottomButtonsRenderer.SetPropertyBlock(botBlock);
    }
}
