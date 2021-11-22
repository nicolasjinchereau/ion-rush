Shader "Custom/CameraShaderDream"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _MainTexMip2 ("Texture (mip 2)", 2D) = "white" { }
        _MainTexMip4 ("Texture (mip 4)", 2D) = "white" { }
        _Power ("Power", Range(0, 10)) = 6
        _Strength ("Strength", Range(0, 1)) = 0.2
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 1.0
        _BlurStrength ("Blur Strength", Range(0, 1)) = 1.0
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
            
            sampler2D _MainTexMip2;
            float4 _MainTexMip2_ST;
            
            sampler2D _MainTexMip5;
            float4 _MainTexMip5_ST;
            
            float4x4 _Mix;
            float _Power;
            float _Strength;
            float _NoiseStrength;
            float _BlurStrength;
            
            float rand(float2 co){
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            
            float4 frag (v2f i) : COLOR
            {
                float glow = dot(float3(0.2126, 0.7152, 0.0722), tex2D(_MainTexMip2, i.uv).rgb);
                glow = pow(glow, _Power) * 10 * _Strength;

                float2 v = i.uv - float2(0.5, 0.5);
                float lenSq = dot(v, v) * 3.0;
                float blur = smoothstep(0, 1, lenSq * lenSq) * _BlurStrength;
                float4 blurred = tex2D(_MainTexMip5, i.uv);

                float noise = rand(i.uv + fmod(_Time.y, 1.0)) * 2.0 - 1.0;
                noise *= 0.07 * _NoiseStrength;

                float4 color = tex2D(_MainTex, i.uv) + glow;
                color = lerp(color, blurred, blur);

                float4 result = mul(_Mix, float4(color.rgb, 1.0)) + noise;
                return result;
            }
            ENDCG
        }
    }
    
    Fallback off
}
