Shader "Custom/CameraShader"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _OverlayTex ("Overlay Texture", 2D) = "white" { }
    }
    
    SubShader
    {
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Lighting off
            Blend One Zero
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            #include "Colorful.cginc"
            
            struct v2f
            {
                float4  pos : SV_POSITION;
                float2  uv : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4x4 _Mix;
            
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            
            float4 frag (v2f i) : COLOR
            {
                float4 color = tex2D(_MainTex, i.uv);
                float4 result = mul(_Mix, float4(color.rgb, 1.0));
                return result;
            }
            ENDCG
        }
    }
    
    Fallback off
}
