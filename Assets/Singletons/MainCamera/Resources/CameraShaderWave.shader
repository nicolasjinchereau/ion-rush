Shader "Custom/CameraShaderWave"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _WavePhaseFreq ("Wave Phase/Freq", Vector) = (0.0, 0.0, 0.0, 0.0)
        _WaveAmplitude ("Wave Amplitude", Vector) = (0.0, 0.0, 0.0, 0.0)
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
            float4 _MainTex_TexelSize;
            float4x4 _Mix;
            
            float4 _WavePhaseFreq;
            float2 _WaveAmplitude;

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            
            float4 frag (v2f i) : COLOR
            {
                // height / width == height * (1 / width)
                float inv_aspect = _MainTex_TexelSize.w * _MainTex_TexelSize.x;
                float twopi = 6.2831853;

                float u = i.uv.x;
                float v = i.uv.y * inv_aspect;
                
                float xoff = sin((_WavePhaseFreq.y * inv_aspect + v * _WavePhaseFreq.w) * twopi) * _WaveAmplitude.y * inv_aspect;
                float yoff = sin((_WavePhaseFreq.x + u * _WavePhaseFreq.z) * twopi) * _WaveAmplitude.x;
                float2 uv = float2(i.uv.x + xoff, i.uv.y + yoff);
                
                float4 color = tex2D(_MainTex, uv);
                float4 result = mul(_Mix, float4(color.rgb, 1.0));
                return result;
            }
            ENDCG
        }
    }
    
    Fallback off
}
