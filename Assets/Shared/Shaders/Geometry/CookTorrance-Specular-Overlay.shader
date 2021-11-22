Shader "Geometry/Cook-Torrance Specular Overlay"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
        _RMS("BDF RMS (Specular spread)",Range(0.001, 1)) = 0.70710678118654752440084436210485
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        
        _HighlightColor ("Highlight Color", Color) = (1, 1, 1, 1)
        _HighlightMix ("Highlight Mix", Range (0, 1)) = 0.5
        _Highlight ("Highlight", Range (0, 1)) = 0.0
        
        _VisibilityTex ("Visibility (R)", 2D) = "white" {}
        _VisibilityThreshold ("Visibility Threshold", Range (0, 1)) = 0.3
        _OverlayOpacity ("Overlay Opacity", Range (0, 1)) = 0.7
    }

    SubShader
    {
        // ROBOT
        Stencil {
            Ref 2
            Comp Always
            Pass replace
        }

        Tags { "Queue"="Geometry+1" }
        Blend One Zero
        Cull back
        ZWrite On
        ZTest LEqual
        
        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf CookTorrance nolightmap vertex:vert //fullforwardshadows //keepalpha
        #include "CookTorrance.cginc"
        #include "Shadows.cginc"
        
        sampler2D _MainTex;
        float4 _HighlightColor;
        float _Highlight;
        float _HighlightMix;
        float _Shininess;
        float _RMS;
        
        inline float4 LightingCookTorrance(SurfaceOutput s, float3 lightDir, float3 viewDir, float atten)
        {
            float4 c = COOK_TORRANCE(s, _LightColor0, _SpecColor, _RMS, lightDir, viewDir, atten);
            c = lerp(c, lerp(c, _HighlightColor, _HighlightMix), _Highlight);
            return c;
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
        };
        
        void vert(inout appdata v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
        }
        
        void surf(Input IN, inout SurfaceOutput o)
        {
            float4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = tex.rgb;
            o.Alpha = tex.a;
            o.Gloss = tex.a;
            o.Specular = _Shininess;
        }
        ENDCG
        
        // OVERLAY
        Stencil {
            Ref 1
            Comp Greater
            Pass replace
        }
        
        Tags { "Queue"="Geometry+1" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull back
        ZWrite On
        ZTest Greater
        Lighting off
        
        CGPROGRAM
        #pragma surface surf Unlit alpha
        sampler2D _VisibilityTex;
        float _VisibilityThreshold;
        float _OverlayOpacity;
        
        inline float4 LightingUnlit(SurfaceOutput s, float3 lightDir, float atten) {
            return float4(s.Albedo, s.Alpha);
        }
        
        struct Input {
            float2 uv_MainTex;
        };
        
        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = float3(0, 0, 0);
            o.Alpha = 1.0 - saturate(tex2Dlod(_VisibilityTex, float4(0.5, 0.5, 0, 0)).r / _VisibilityThreshold);
            o.Alpha *= _OverlayOpacity;
            o.Specular = 0;
            o.Gloss = 0;
        }
        ENDCG
    }

    FallBack "Specular"
}
