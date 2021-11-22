// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Display Static"
{
    Properties
    {
        _ColorA("Color A", Color) = (1, 1, 1, 1)
        _ColorB("Color B", Color) = (0, 0, 0, 0)
        _BlockCountX("Block Count X", Float) = 2
        _BlockCountY("Block Count Y", Float) = 2
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            float rand(float2 co)
            {
                return frac((sin(dot(co.xy, float2(12.345 * _Time.w, 67.890 * _Time.w))) * 12345.67890 + _Time.w));
            }
            
            float4 _ColorA;
            float4 _ColorB;
            float _BlockCountX;
            float _BlockCountY;

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float blockSizeX = 1.0 / _BlockCountX;
                float blockSizeY = 1.0 / _BlockCountY;

                float x = int(i.uv.x / blockSizeX) * blockSizeX;
                float y = int(i.uv.y / blockSizeY) * blockSizeY;

                float noise = rand(float2(x, y));
                float4 stat = lerp(_ColorA, _ColorB, noise);
                return float4(stat.xyz, 1.0);
            }

             ENDCG
         }
    }
}
