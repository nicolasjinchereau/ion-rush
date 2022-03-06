Shader "Geometry/Cook-Torrance Specular Dissolve" {
Properties
{
    _MainTex ("Main Texture", 2D) = "white" {}
    _Color ("Main Color", Color) = (1, 1, 1, 1)
    [Header(Specular)][Space(10)]
    _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
    _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
    _RMS ("Specular Spread (BDF RMS)",Range(0.001, 1)) = 0.70710678118654752440084436210485
    [Header(Emission)][Space(10)]
    [HDR]_EmissionColor("Emission Color", Color) = (0, 0, 0, 0)
    _EmissionAmount("Emission Amount", Range(0, 1)) = 1.0
    [Header(Dissolve)][Space(10)]
    _Noise ("Noise", 2D) = "white" {}
    [HDR] _EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
    _EdgeSize ("EdgeSize", Range(0, 1)) = 0.2
    _Cutoff ("Cutoff", Range(0, 1)) = 0.0
    [Header(Refraction)][Space(10)]
    _GrabOffset("Grab Offset", Range(-1.0, 1.0)) = 0.0
    _Flatten ("Flatten", Range(0, 1)) = 0.0
}

SubShader
{
    Tags {
        "Queue" = "AlphaTest"
        "RenderType" = "TransparentCutout"
    }
    
    LOD 200
    Blend Off
    Cull Back
    
    GrabPass
    {
        Tags { "LightMode" = "Always" }
        "_GrabTexture"
    }

    CGPROGRAM
    #pragma target 3.0
    #pragma shader_feature PLAYER_SHADOW_ON
    #pragma surface surf CookTorrance vertex:vert fullforwardshadows
    
    #include "CookTorrance.cginc"
    #include "Shadows.cginc"
    #include "UnityCG.cginc"
    
    sampler2D _MainTex;
    float _Shininess;
    float _RMS;

    sampler2D _GrabTexture;
    sampler2D _Noise;
    float4 _EmissionColor;
    float _EmissionAmount;
    float4 _EdgeColor;
    float _EdgeSize;
    float _GrabOffset;
    float _Cutoff;
    float _Flatten;

    float4 LightingCookTorrance(SurfaceOutputPlayerShadow s, float3 lightDir, float3 viewDir, float atten)
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
        float2 uv_Noise;
        float2 uv_MainTex;
        float3 normal;
        float4 grabPos;
    };

    void vert(inout appdata v, out Input o)
    {
        UNITY_INITIALIZE_OUTPUT(Input, o);

        o.normal = mul(unity_ObjectToWorld, float4(SCALED_NORMAL, 0.0)).xyz;
        o.normal = mul(unity_MatrixV, float4(o.normal, 0.0)).xyz;
        o.grabPos = ComputeGrabScreenPos(UnityObjectToClipPos(v.vertex));
    }

    void surf(Input IN, inout SurfaceOutputPlayerShadow o)
    {
        float4 tex = tex2D(_MainTex, IN.uv_MainTex);
        float noise = tex2D(_Noise, IN.uv_Noise).r;
        
        _Cutoff = lerp(0, _Cutoff + _EdgeSize, _Cutoff);
        float edge = smoothstep(_Cutoff + _EdgeSize, _Cutoff, clamp(noise, _EdgeSize, 1));

        float3 normal = normalize(IN.normal);
        normal.y = -normal.y;
        float len = length(normal.xy);

        //float s = (1.0 - len) * len;
        float s = len * len;

        float bulge = 1.0 - _Flatten;
        s *= smoothstep(lerp(-0.4, 1.0, _Flatten), lerp(0.0, 1.4, _Flatten), (1.0 - len));

        IN.grabPos.xy += -normalize(normal.xy) * _GrabOffset * s;
        float3 bg = tex2Dproj(_GrabTexture, IN.grabPos).rgb;
        bg += (_EmissionColor.rgb * _EmissionAmount * tex.a);

        float3 col = lerp(tex.rgb, float3(0, 0, 0), step(0, _Cutoff - noise));

        o.Albedo = col;
        o.Alpha = tex.a;
        o.Gloss = tex.a;
        o.Specular = _Shininess;
        o.Emission = lerp(bg.rgb, _EdgeColor * edge, step(0, noise - _Cutoff));
    }
    ENDCG
    }

    FallBack "Specular"
}
