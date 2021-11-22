Shader "Geometry/Cook-Torrance Specular Reflective" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
    _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
    _RMS("BDF RMS (Specular spread)",Range(0.001,1)) = 0.70710678118654752440084436210485
    _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}

    _Cube("Reflection Cubemap", Cube) = "_Skybox" {}
    _ReflectColor("Reflection Color", Color) = (1,1,1,0.5)
    [HDR] _EmissiveColor("Emissive Color", Color) = (0, 0, 0, 0)
}

SubShader {
    Tags { "RenderType"="Opaque" }
    Blend One Zero
    LOD 200
    
    CGPROGRAM
    #pragma target 3.0
    #pragma surface surf CookTorrance vertex:vert fullforwardshadows
    
    #include "UnityCG.cginc"
    #include "CookTorrance.cginc"
    
    sampler2D _MainTex;
    samplerCUBE _Cube;
    float _Shininess;
    float _RMS;
    float4 _Color;
    float4 _ReflectColor;
    float4 _EmissiveColor;

    float4 LightingCookTorrance(SurfaceOutput s, float3 lightDir, float3 viewDir, float atten)
    {
        return COOK_TORRANCE(s, _LightColor0, _SpecColor, _RMS, lightDir, viewDir, atten);
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
        float3 worldRefl;
    };

    void vert(inout appdata v, out Input o)
    {
        UNITY_INITIALIZE_OUTPUT(Input, o);
        float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
    }

    void surf(Input IN, inout SurfaceOutput o)
    {
        float4 tex = tex2D(_MainTex, IN.uv_MainTex);
        tex *= _Color;

        float4 reflcol = texCUBE(_Cube, IN.worldRefl);
        reflcol.rgb *= _ReflectColor.rgb;

        o.Albedo = lerp(tex.rgb, reflcol.rgb, _ReflectColor.a);
        o.Alpha = tex.a;
        o.Gloss = 1;
        o.Specular = _Shininess;
        o.Emission = _EmissiveColor.rgb;
    }
    ENDCG
    }

    FallBack "Specular"
}
