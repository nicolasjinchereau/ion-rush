Shader "Custom/UnlitSolid"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" }
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
            
            float4 _Color;
            
            struct appdata_t {
                float4 vertex : POSITION;
            };
            
            struct v2f {
                float4 vertex : POSITION;
            };
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            float4 frag (v2f i) : COLOR {
                return _Color;
            }
            ENDCG 
        }
    }
}
