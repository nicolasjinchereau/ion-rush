Shader "Geometry/Cook-Torrance Specular Highlighted"
{
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
    _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
    _RMS("BDF RMS (Specular spread)",Range(0.001,1)) = 0.70710678118654752440084436210485
    _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
    
    _HighlightColor ("Highlight Color", Color) = (1, 1, 1, 1)
    _HighlightRimPower("Highlight Rim Power", Range(0.5, 10.0)) = 2.0
    _HighlightRimMix("Highlight Mix", Range(0, 1)) = 0.5
    _Highlight("Highlight", Range(0, 1)) = 0.0

    [Toggle(PLAYER_SHADOW_ON)] _PlayerShadow ("Player Shadow", Float) = 0
}

SubShader
{
    Tags { "RenderType"="Opaque" "ReceivePlayerShadow"="True" }
    LOD 200
    
    CGPROGRAM
    #pragma target 3.0
    #pragma shader_feature PLAYER_SHADOW_ON
    #pragma surface surf CookTorrance nolightmap fullforwardshadows vertex:vert

    #include "UnityCG.cginc"
    #include "CookTorrance.cginc"
    #include "Shadows.cginc"

    sampler2D _MainTex;
    float4 _Color;
    float _Shininess;
    float _RMS;

    float4 _HighlightColor;
    float _Highlight;
    float _HighlightRimMix;
    float _HighlightRimPower;

    PLAYER_SHADOW_UNIFORMS

    inline float4 LightingCookTorrance(SurfaceOutputPlayerShadow s, float3 lightDir, float3 viewDir, float atten)
    {
        atten = min(atten, PLAYER_SHADOW_ATTEN(s));
        return COOK_TORRANCE(s, _LightColor0, _SpecColor, _RMS, lightDir, viewDir, atten);
    }
    
    struct appdata
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float4 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Input {
        float2 uv_MainTex;
        float3 viewDir;
        PLAYER_SHADOW_V2S
    };

    void vert(inout appdata v, out Input o) {
        UNITY_INITIALIZE_OUTPUT(Input, o);
        PLAYER_SHADOW_VERT_TO_FRAG(v.vertex, v.normal, o)
    }

    void surf(Input IN, inout SurfaceOutputPlayerShadow o)
    {
        float4 tex = tex2D(_MainTex, IN.uv_MainTex);
        o.Albedo = tex.rgb * _Color.rgb;
        o.Gloss = tex.a;
        o.Alpha = tex.a * _Color.a;
        o.Specular = _Shininess;
        PLAYER_SHADOW_SURFACE_INOUT(IN, o);

        float rimLight = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
        float3 rimColor = _HighlightColor.rgb * pow(rimLight, _HighlightRimPower);
        float3 highlightColor = lerp(_HighlightColor.rgb, rimColor, _HighlightRimMix);
        float3 emmColor = lerp(float3(0, 0, 0), highlightColor, _Highlight);
        o.Emission = emmColor;
    }
    ENDCG
    }

    FallBack "Specular"
}
