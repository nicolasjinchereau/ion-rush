Shader "Geometry/Diffuse-Transparent"
{
	Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
    
	SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        
        LOD 200
        ZWrite off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
		    CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _LightColor0;
            float4 _Color;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };
            
            v2f vert(appdata v)
            {
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
                
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = mul((float3x3)unity_ObjectToWorld, SCALED_NORMAL);
                return o;
            }
            
            float4 frag(v2f i) : COLOR
            {
                return tex2D(_MainTex, i.uv) * _Color;
            }
		    ENDCG
        }
	}
    
	FallBack "Diffuse"
}
