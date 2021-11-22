Shader "Custom/RobotOcclusionShader"
{
    Properties { }
    
    // OcclusionType = 0: geometry that can occlude player robot
    // OcclusionType = 1: the player robot
    SubShader
    {
        Tags { "Queue" = "Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" "OcclusionType"="0" }
        Lighting Off
        ZWrite on
        ZTest less
        Cull back
        Blend One Zero
        
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
                return float4(0, 0, 0, 0);
            }
            ENDCG
        }
    }

    SubShader
    {
        Tags { "Queue" = "Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" "OcclusionType"="1" }
        Lighting Off
        ZWrite on
        ZTest less
        Cull back
        Blend One Zero
        
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
                return float4(1, 1, 1, 1);
            }
            ENDCG 
        }
    }
}
