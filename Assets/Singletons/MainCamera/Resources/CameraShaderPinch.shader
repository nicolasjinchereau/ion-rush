Shader "Custom/CameraShaderPinch"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _PinchRadius ("Pinch Radius", Range(0.0, 1.0)) = 1.0 // 1.0 is half screen height
        _PinchOffset("Pinch Offset", Range(0.0, 0.5)) = 0.1
        _PinchAmplitude ("Pinch Amplitude", Range(-2, 2)) = 0.0
        _PinchPower ("Pinch Power", Range(1, 10)) = 1.0
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
            
            struct v2f
            {
                float4  pos : SV_POSITION;
                float2  uv : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize; // 1 / width,  1 / height,  width,  height
            float4x4 _Mix;
            
            float _PinchRadius;
            float _PinchOffset;
            float _PinchAmplitude;
            float _PinchPower; // unused

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            
            float4 frag(v2f i) : COLOR
            {
                // height / width == height * (1 / width)
                float inv_aspect = _MainTex_TexelSize.w * _MainTex_TexelSize.x;
                float PI = 3.141592654;

                float2 pos = i.uv * 2.0 - 1.0;
                pos.y *= inv_aspect;

                float len = length(pos);
                float2 dir = pos / len;

                float x = clamp(len, 0, _PinchRadius) / _PinchRadius;
                float off = clamp(pow(1.0 - x, _PinchPower), 0, 1) * _PinchOffset * _PinchAmplitude * len;

                float2 uv = i.uv + dir * off;

                float4 color = tex2D(_MainTex, uv);
                float4 result = mul(_Mix, float4(color.rgb, 1.0));
                return result;
            }
            ENDCG
        }
    }
    
    Fallback off
}
