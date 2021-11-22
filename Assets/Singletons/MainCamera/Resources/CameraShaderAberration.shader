Shader "Custom/CameraShaderAberration"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _RedOffset ("Red Offset", Vector) = (0, 0, 0, 0)
        _GreenOffset ("Green Offset", Vector) = (0, 0, 0, 0)
        _BlueOffset ("Blue Offset", Vector) = (0, 0, 0, 0)
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
            float4 _MainTex_TexelSize;
            float2 _RedOffset;
            float2 _GreenOffset;
            float2 _BlueOffset;
            
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            
            float4 frag(v2f i) : COLOR
            {
                float invAspect = _MainTex_TexelSize.w / _MainTex_TexelSize.z;
                float2 redOffset = float2(_RedOffset.x * invAspect, _RedOffset.y);
                float2 greenOffset = float2(_GreenOffset.x * invAspect, _GreenOffset.y);
                float2 blueOffset = float2(_BlueOffset.x * invAspect, _BlueOffset.y);
                
                float r = tex2D(_MainTex, i.uv + redOffset).r;
                float g = tex2D(_MainTex, i.uv + greenOffset).g;
                float b = tex2D(_MainTex, i.uv + blueOffset).b;
                float a = tex2D(_MainTex, i.uv).a;

                float4 col = float4(r, g, b, a);

                return col;
            }
            ENDCG
        }
    }
    
    Fallback off
}
