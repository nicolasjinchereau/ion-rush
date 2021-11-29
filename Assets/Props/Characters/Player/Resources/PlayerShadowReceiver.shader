Shader "Custom/PlayerShadowReceiver"
{
    Properties
    {
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "ReceivePlayerShadow"="True" }
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
            
            struct appdata_t {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : POSITION;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : COLOR {
                return float4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
