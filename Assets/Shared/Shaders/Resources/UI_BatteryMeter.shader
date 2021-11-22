// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/UI_BatteryMeter"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _BatteryLevel ("Battery Level", Range(0, 1)) = 1
    }

    SubShader
    {
        LOD 0
        Tags {"Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Overlay"}

        Pass
        {
            Name "FORWARD"
            Cull off
            Lighting off
            ZTest always
            ZWrite off
            Fog { Mode Off }
            Blend One OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BatteryLevel;

            struct UIVertex {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float4 col : COLOR;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                //float4 col : COLOR;
            };

            v2f vert(UIVertex v)
            {
                v2f o;

                float4 pos = v.pos;
                float2 tex = v.uv;

                if(pos.x > 0)
                    pos.x -= (pos.x * 2) * (1.0 - _BatteryLevel);
                
                tex.x *= _BatteryLevel;
                
                o.pos = UnityObjectToClipPos(pos);
                o.uv = TRANSFORM_TEX(tex, _MainTex);
                //o.col = v.col;
                return o;
            }
            
            float4 frag(v2f i) : COLOR0
            {
                float4 col = tex2D(_MainTex, i.uv) * _Color;
                return float4(col.rgb * col.a, col.a);
            }
            ENDCG
        }
    }

    Fallback "Transparent/Diffuse"
}
