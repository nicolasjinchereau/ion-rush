Shader "Geometry/Cook-Torrance Specular Mask Cutout" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
    _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
    _RMS("BDF RMS (Specular spread)",Range(0.001,1)) = 0.70710678118654752440084436210485
    _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
    _MaskTex("(R) Cutoff Mask", 2D) = "white" {}
    _Cutoff("Alpha cutoff", Range(0, 1)) = 0
}

SubShader {
    Tags {
        "RenderType"="Geometry"
        "Queue"="Geometry"
    }

    LOD 200
    Blend One Zero
    Cull back
    ZWrite On
    ZTest Less

    CGPROGRAM
    #pragma target 3.0
    #pragma surface surf CookTorrance vertex:vert fullforwardshadows keepalpha
    
    #include "UnityCG.cginc"
    #include "CookTorrance.cginc"
    #include "Shadows.cginc"
    
    sampler2D _MainTex;
    sampler2D _MaskTex;
    float _Shininess;
    float _RMS;
    float4 _Color;
    float _Cutoff;

    float4 LightingCookTorrance(SurfaceOutputPlayerShadow s, float3 lightDir, float3 viewDir, float atten)
    {
        float4 c = COOK_TORRANCE(s, _LightColor0, _SpecColor, _RMS, lightDir, viewDir, atten);
        return c;
    }
    
    struct appdata
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float4 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Input
    {
        float2 uv_MainTex;
        float2 uv_MaskTex;
        PLAYER_SHADOW_V2S
    };

    void vert(inout appdata v, out Input o)
    {
        UNITY_INITIALIZE_OUTPUT(Input, o);
        float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
    }

    void surf(Input IN, inout SurfaceOutputPlayerShadow o)
    {
        float4 mask = tex2D(_MaskTex, IN.uv_MaskTex);
        clip(mask.r - _Cutoff);

        float4 tex = tex2D(_MainTex, IN.uv_MainTex);
        o.Albedo = tex.rgb * _Color.rgb;
        o.Alpha = tex.a * _Color.a;
        o.Gloss = tex.a;
        o.Specular = _Shininess;
    }
    ENDCG
    }

    FallBack "Specular"
}
