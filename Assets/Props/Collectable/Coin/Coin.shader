// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/Coin" {
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
	}
    
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		        
		CGPROGRAM
		#pragma surface surf Lambert fullforwardshadows nolightmap
		#pragma target 3.0
        
		struct Input {
			float2 uv_MainTex;
		};
        
		float4 _Color;
        
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)
        
		void surf(Input IN, inout SurfaceOutput o)
        {
			o.Albedo = _Color.rgb;
            o.Alpha = _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
