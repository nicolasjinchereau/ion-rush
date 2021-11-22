Shader "Custom/EventHorizon"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0.5, 10.0)) = 3.0
        _Illumination("Illumination", Range(0.0, 1.0)) = 0.0
        _Emission ("Emission", Color) = (1,1,1,1)
    }
    SubShader
    {
        LOD 100
        Tags { "Queue"="Transparent+1" "RenderType" = "Transparent" }
        
        ZTest less
        ZWrite on

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = mul((float3x3)unity_ObjectToWorld, SCALED_NORMAL);
                o.worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));

                return o;
            }

            float4 _Color;
            float4 _RimColor;
            float _RimPower;
            float _Illumination;
            float4 _Emission;

            float4 frag(v2f i) : COLOR
            {
                float4 col = tex2D(_MainTex, i.uv);
                col *= _Color;

                float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);
                float rim = 1.0 - saturate(dot(-viewDir, normalize(i.normal)));
                float4 rimColor = _RimColor * pow(rim, _RimPower);

                col = lerp(col, rimColor, rimColor.a * _Illumination);
                col.rgb *= (2.0 + 3.0 * _Illumination);
                col += _Emission;
                return col;
            }
            ENDCG
        }
    }
}
