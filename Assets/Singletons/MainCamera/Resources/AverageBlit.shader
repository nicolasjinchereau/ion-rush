// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/AverageBlit"
{
    Properties {
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
    }
    
    SubShader {
        Tags { "Queue" = "Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" }
        Lighting Off
        ZWrite off
        ZTest always
        Cull Off
        Blend One Zero
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata_t {
                float4 vertex : POSITION;
            };
            
            struct v2f {
                float4 vertex : POSITION;
            };
            
            sampler2D _MainTex;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            float4 frag (v2f i) : COLOR
            {
                float total = 0;
                float inv = 1.0 / 64.0;
                
                for(int y = 0; y < 64; ++y) {
                    for(int x = 0; x < 64; ++x) {
                        total += tex2Dlod(_MainTex, float4((x + 0.5) * inv, (y + 0.5) * inv, 0, 0)).r;
                    }
                }
                
                total = total / (64 * 64) * 7.0;
                return float4(total, total, total, total);
            }
            ENDCG
        }
    }
}
