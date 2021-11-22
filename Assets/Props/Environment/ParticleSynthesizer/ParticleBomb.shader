Shader "Custom/Particle Bomb"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.5, 10.0)) = 3.0
    }
    
    SubShader
    {
        LOD 200
        Tags { "Queue"="Transparent+1" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
        
            struct v2f
            {
                float4 pos : POSITION;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };
            
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = mul((float3x3)unity_ObjectToWorld, SCALED_NORMAL);
                o.worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
                return o;
            }
            
            float4 _Color;
            float4 _RimColor;
            float _RimPower;

            float4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);
                float rim = 1.0 - saturate(dot(normalize(-viewDir), normalize(i.normal)));
                float rimVal = pow(rim, _RimPower);

                float4 ret = _RimColor * rimVal;
                ret.a = rimVal;
                
                ret = max(ret, _Color * _Color.a);

                return ret;
            }
            ENDCG
        }
    }

    Fallback "VertexLit"
}
