Shader "Geometry/Cook-Torrance Specular Refractive"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
        _RMS("BDF RMS (Specular spread)",Range(0.001, 1)) = 0.70710678118654752440084436210485
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        
        _Phase ("Phase", Range(0, 1)) = 0.0
        _GrabOffset ("Grab Offset", Range(0, 2.0)) = 0.5

        //_HighlightColor ("Highlight Color", Color) = (1, 1, 1, 1)
        //_HighlightMix ("Highlight Mix", Range (0, 1)) = 0.5
        //_Highlight ("Highlight", Range (0, 1)) = 0.0
        //_VisibilityTex ("Visibility (R)", 2D) = "white" {}
        //_VisibilityThreshold ("Visibility Threshold", Range (0, 1)) = 0.3
        //_OverlayOpacity ("Overlay Opacity", Range (0, 1)) = 0.7
    }

    SubShader
    {
        Tags { "Queue"="Transparent+1000" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull back
        ZWrite On
        ZTest LEqual
        
        GrabPass
        {
            Tags { "LightMode" = "Always" }
            "_GrabTexture"
        }

        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf CookTorrance nolightmap vertex:vert keepalpha //fullforwardshadows
        #include "CookTorrance.cginc"
        #include "Shadows.cginc"
        #include "UnityCG.cginc"

        float4 _Color;
        sampler2D _MainTex;
        float _Shininess;
        float _RMS;
        float _Phase;
        float _GrabOffset;
        sampler2D _GrabTexture;

        inline float4 LightingCookTorrance(SurfaceOutput s, float3 lightDir, float3 viewDir, float atten)
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
        
        struct Input {
            float2 uv_MainTex;

            float3 normal : NORMAL;
            float3 pivotWorldPos : TEXCOORD1;
            float3 vertexWorldPos : TEXCOORD2;
            float4 grabPos : TEXCOORD3;
        };
        
        void vert(inout appdata v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            o.normal = mul((float3x3)unity_ObjectToWorld, SCALED_NORMAL);
            o.pivotWorldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
            o.vertexWorldPos = mul(unity_ObjectToWorld, v.vertex);

            float4 pos = UnityObjectToClipPos(v.vertex);
            o.grabPos = ComputeGrabScreenPos(pos);
        }
        
        void surf(Input IN, inout SurfaceOutput o)
        {
            float3 camDir = normalize(_WorldSpaceCameraPos - IN.pivotWorldPos);
            float3 normal = normalize(IN.normal);
            IN.grabPos.xy -= normal.xy * _GrabOffset;
            float4 bg = tex2Dproj(_GrabTexture, IN.grabPos);

            float4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = lerp(tex.rgb, bg.rgb, _Phase) * _Color.rgb;
            o.Alpha = _Color.a;
            o.Gloss = tex.a;
            o.Specular = _Shininess;
        }
        ENDCG
    }

    FallBack "Specular"
}
