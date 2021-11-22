Shader "Custom/QuantumComputerButtons"
{
    Properties
    {
        _OnColor("On Color", Color) = (0, 0, 1.0, 1)
        _OffColor("Off Color", Color) = (0, 0, 0.5, 1)
        [PerRendererData] _TimeOffset("Time Offset", Float) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" }
        Lighting Off
        ZWrite on
        ZTest less
        Cull back
        Blend off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 _OnColor;
            float4 _OffColor;
            float _TimeOffset;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            float rand(float2 co, float time)
            {
                return frac((sin(dot(co.xy, float2(12.345 * time, 67.890 * time))) * 12345.67890 + time));
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            
            float4 frag(v2f i) : COLOR
            {
                int x = min(int(i.uv.x * 4), 3);
                int y = min(int(i.uv.y * 4), 3);
                float val = step(0.5, rand(float2(x, y), round((_TimeOffset + _Time.w) / 0.25)));
                return lerp(_OffColor, _OnColor, val);
            }
            ENDCG 
        }
    }
}
