// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Slipstream Badge"
{
    Properties{
    _Color("Main Color", Color) = (1, 1, 1, 1)
    [HDR] _ReflectColor("Reflection Color", Color) = (1, 1, 1, 0.5)
    [HDR] _EmissiveColor("Emissive Color", Color) = (0, 0, 0, 0)
    _MainTex ("Base (RGB) RefStrength (A)", 2D) = "white" {}
    _Cube ("Reflection Cubemap", Cube) = "_Skybox" {}
}
SubShader {
    LOD 200
    Tags { "RenderType"="Opaque" }

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
samplerCUBE _Cube;

float4 _Color;
float4 _ReflectColor;
float4 _EmissiveColor;

struct Input {
    float2 uv_MainTex;
    float3 worldRefl;
};

void surf (Input IN, inout SurfaceOutput o) {
    float4 col = tex2D(_MainTex, IN.uv_MainTex);
    float4 ref = texCUBE(_Cube, IN.worldRefl);
    
    col.rgb *= _Color.rgb;
    ref.rgb *= _ReflectColor.rgb;
    
    float4 c = col * _Color;
    o.Albedo = lerp(col.rgb, col.rgb * ref.rgb, col.a * _ReflectColor.a);
    o.Emission = _EmissiveColor.rgb * col.a;
    o.Alpha = 1;
}
ENDCG
}

FallBack "Legacy Shaders/Reflective/VertexLit"
}
