// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/ReflectiveGlass" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _Cube ("Reflection Cubemap", Cube) = "_Skybox" {}
}
SubShader {
    LOD 200
    Tags {
        "Queue"="Transparent"
        "RenderType"="Transparent"
    }
    Fog { Mode Off }
    Blend SrcAlpha OneMinusSrcAlpha
    ZWrite off

CGPROGRAM
#pragma surface surf Lambert keepalpha

float4 _Color;
samplerCUBE _Cube;

struct Input {
    float3 worldRefl;
};

void surf (Input IN, inout SurfaceOutput o)
{
    float4 col = texCUBE(_Cube, IN.worldRefl);
    col *= _Color;

    o.Albedo = col.rgb;
    o.Alpha = col.a;
    //o.Emission = float4(0, 0, 0, 0);
}
ENDCG
}

FallBack "Legacy Shaders/Reflective/VertexLit"
}
