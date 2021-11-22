using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RobotOcclusionCamera : MonoBehaviour
{
    public Material robotMaterial;
    public Camera cam;
    public Material[] occluders = new Material[0];
    
    RenderTexture mask;
    RenderTexture visibility;
    Material averageBlitMat;
    
    private void OnEnable() {
        UpdateResources();
    }

    private void OnDisable()
    {
        if(cam) {
            cam.targetTexture = null;
        }

        if (mask) {
            mask.Release();
            mask = null;
        }

        if (robotMaterial) {
            robotMaterial.SetTexture("_VisibilityTex", null);
        }

        if (visibility) {
            visibility.Release();
            visibility = null;
        }
    }

    void UpdateResources()
    {
        if(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat))
        {
            mask = new RenderTexture(64, 64, 16, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            visibility = new RenderTexture(1, 1, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        }
        else if(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8))
        {
            mask = new RenderTexture(64, 64, 16, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
            visibility = new RenderTexture(1, 1, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
        }
        else if(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
        {
            mask = new RenderTexture(64, 64, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            visibility = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        }
        else
        {
            Debug.Log("Failed to create render texture for occlusion - insufficient render texture support.");
            gameObject.SetActive(false);
            return;
        }

        mask.filterMode = FilterMode.Point;
        visibility.filterMode = FilterMode.Point;
        averageBlitMat = new Material(Shader.Find("Custom/AverageBlit"));

        cam = GetComponent<Camera>();
        cam.targetTexture = mask;
        cam.SetReplacementShader(Shader.Find("Custom/RobotOcclusionShader"), "OcclusionType");
        cam.eventMask = 0;

        robotMaterial.SetTexture("_VisibilityTex", visibility);

        foreach (var mat in occluders)
            mat.SetOverrideTag("OcclusionType", "0");
    }

    void LateUpdate()
    {
        if(Player.exists)
            transform.rotation = Quaternion.LookRotation((Player.position + Vector3.up * 0.5f - transform.position).normalized);
    }
    
    void OnPostRender() {
        Blit(mask, visibility, averageBlitMat);
    }
    
    protected void Blit(Texture source, RenderTexture target, Material mat)
    {
        Graphics.SetRenderTarget(target);
        
        mat.SetTexture("_MainTex", source);
        mat.SetPass(0);
        
        float w = source.width;
        float h = source.height;
        
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Viewport(new Rect(0, 0, 1, 1));
        GL.Clear(false, true, Color.clear);
        GL.Begin(GL.TRIANGLE_STRIP);
        {
            GL.TexCoord2(0, 1);
            GL.Vertex3(0, 1, 0);

            GL.TexCoord2(0, 0);
            GL.Vertex3(0, 0, 0);

            GL.TexCoord2(1, 1);
            GL.Vertex3(1, 1, 0);

            GL.TexCoord2(1, 0);
            GL.Vertex3(1, 0, 0);
        }
        GL.End();
        GL.PopMatrix();
        
        Graphics.SetRenderTarget(null);
    }
}
