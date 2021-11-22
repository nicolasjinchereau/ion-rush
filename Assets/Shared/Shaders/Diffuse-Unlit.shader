Shader "Custom/Diffuse Unlit"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }

    SubShader
    {
        LOD 50
        Tags { "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" }

        Pass
        {
            Fog { Mode Off }
            Blend One Zero
            ZWrite On
            ZTest LEqual
            Lighting Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }
            
            float4 frag(v2f i) : COLOR0
            {
                float4 col = tex2D(_MainTex, i.uv);
                return col * _Color;
            }
            ENDCG
        }
    }

    Fallback "Transparent/VertexLit"
}
