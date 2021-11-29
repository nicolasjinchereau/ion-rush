using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class ShadowCaster : MonoBehaviour
{
    public int targetSize = 512;
    public float shadowBias = 0.005f;
    public float shadowFalloff = 0.2f;
    public float shadowFalloffPower = 4.0f;
    public Camera playerCam;
    public Camera worldCam;
    
    private RenderTexture playerDepth;
    private RenderTexture worldDepth;

    private void OnEnable() {
        UpdateResources();
    }

    private void OnDisable()
    {
        if(playerCam) {
            playerCam.targetTexture = null;
        }

        if(worldCam) {
            worldCam.targetTexture = null;
        }

        if(playerDepth) {
            playerDepth.Release();
            playerDepth = null;
        }

        if(worldDepth) {
            worldDepth.Release();
            worldDepth = null;
        }
    }

    private void UpdateResources()
    {
        playerCam.depth = -100;
        playerCam.eventMask = 0;
        worldCam.depth = -100;
        worldCam.eventMask = 0;

        int sz = Mathf.Max(targetSize, 16);

        if(!playerDepth)
        {
            playerDepth = new RenderTexture(sz, sz, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            playerDepth.wrapMode = TextureWrapMode.Clamp;
            playerDepth.filterMode = FilterMode.Bilinear;
            playerDepth.autoGenerateMips = false;
            playerDepth.useMipMap = false;
        }

        playerCam.targetTexture = playerDepth;
        
        if(!worldDepth)
        {
            worldDepth = new RenderTexture(sz, sz, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            worldDepth.wrapMode = TextureWrapMode.Clamp;
            worldDepth.filterMode = FilterMode.Bilinear;
            worldDepth.autoGenerateMips = false;
            worldDepth.useMipMap = false;
        }

        worldCam.targetTexture = worldDepth;
        worldCam.SetReplacementShader(Shader.Find("Custom/PlayerShadowReceiver"), "ReceivePlayerShadow");
        
        shadowFalloff = Mathf.Clamp01(shadowFalloff);
    }
    
    private void OnPreRender()
    {
        var bias = new Matrix4x4() {
            m00 = 0.5f, m01 = 0,    m02 = 0,    m03 = 0.5f,
            m10 = 0,    m11 = 0.5f, m12 = 0,    m13 = 0.5f,
            m20 = 0,    m21 = 0,    m22 = 0.5f, m23 = 0.5f,
            m30 = 0,    m31 = 0,    m32 = 0,    m33 = 1,
        };
        
        Matrix4x4 playerMtx = bias * playerCam.projectionMatrix * playerCam.worldToCameraMatrix;
        Matrix4x4 worldMtx = bias * worldCam.projectionMatrix * worldCam.worldToCameraMatrix;
        
        Shader.SetGlobalMatrix("_PlayerMatrix", playerMtx);
        Shader.SetGlobalMatrix("_WorldMatrix", worldMtx);
        Shader.SetGlobalTexture("_PlayerDepthTex", playerDepth);
        Shader.SetGlobalTexture("_WorldDepthTex", worldDepth);
        
        Vector4 shadowParams;
        shadowParams.x = shadowBias;
        shadowParams.y = MainLight.Light.shadowStrength;
        shadowParams.z = shadowFalloff;
        shadowParams.w = shadowFalloffPower;
        Shader.SetGlobalVector("_PlayerShadowParams", shadowParams);
    }
}
