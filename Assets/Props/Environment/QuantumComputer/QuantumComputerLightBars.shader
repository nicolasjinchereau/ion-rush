Shader "Custom/QuantumComputerLightBars"
{
    Properties
    {
        _OnColor("On Color", Color) = (0, 0, 1.0, 1)
        _OffColor ("Off Color", Color) = (0, 0, 0.5, 1)
        _BlinkSpeed("Blink Speed", Float) = 1.0
        [PerRendererData] _TimeOffset ("Time Offset", Float) = 0.0
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
            float _BlinkSpeed;
            float _TimeOffset;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            
            float4 frag(v2f i) : COLOR
            {
                float x = (_TimeOffset + _Time.w) * _BlinkSpeed;
                float y = abs(fmod(x, 2.0) - 1.0);
                return lerp(_OffColor, _OnColor, y);
            }
            ENDCG 
        }
    }
}
