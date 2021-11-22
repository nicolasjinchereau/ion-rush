Shader "Custom/DownsampleX2"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
			
			#include "UnityCG.cginc"
            
			struct VSInput
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
            
			v2f vert(VSInput v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                float2 off = _MainTex_TexelSize.xy;
                o.uv0 = v.uv + float2(-off.x,  off.y);
                o.uv1 = v.uv + float2( off.x,  off.y);
                o.uv2 = v.uv + float2(-off.x, -off.y);
                o.uv3 = v.uv + float2( off.x, -off.y);
				return o;
			}
			
			float4 frag(v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv0);
                col += tex2D(_MainTex, i.uv1);
                col += tex2D(_MainTex, i.uv2);
                col += tex2D(_MainTex, i.uv3);
				return col * 0.25;
			}
			ENDCG
		}
	}
}
