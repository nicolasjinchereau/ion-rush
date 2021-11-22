// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'

Shader "FX/Water" { 
Properties {
    _WaveScale ("Wave scale", Range (0.02,0.15)) = 0.063
    _ReflDistort ("Reflection distort", Range (0,1.5)) = 0.44
    _RefrDistort ("Refraction distort", Range (0,1.5)) = 0.40
    _RefrColor ("Refraction color", COLOR)  = ( .34, .85, .92, 1)
    _Fresnel ("Fresnel (A) ", 2D) = "gray" {}
    _BumpMap ("Normalmap ", 2D) = "bump" {}
    WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
    _ReflectiveColor ("Reflective color (RGB) fresnel (A) ", 2D) = "" {}
    _ReflectiveColorCube ("Reflective color cube (RGB) fresnel (A)", Cube) = "" { TexGen CubeReflect }
    _HorizonColor ("Simple water horizon color", COLOR)  = ( .172, .463, .435, 1)
    _MainTex ("Fallback texture", 2D) = "" {}
    _ReflectionTex ("Internal Reflection", 2D) = "" {}
    _RefractionTex ("Internal Refraction", 2D) = "" {}
}


// -----------------------------------------------------------
// Fragment program cards


Subshader { 
    Tags { "Queue"="Transparent" "WaterMode"="Refractive" "RenderType"="Transparent" }
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest 
        #pragma multi_compile WATER_REFRACTIVE WATER_REFLECTIVE WATER_SIMPLE

        #if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
            #define HAS_REFLECTION 1
        #endif

        #if defined (WATER_REFRACTIVE)
            #define HAS_REFRACTION 1
        #endif

        #include "UnityCG.cginc"

        uniform float4 _WaveScale4;
        uniform float4 _WaveOffset;

        #if HAS_REFLECTION
            uniform float _ReflDistort;
        #endif

        #if HAS_REFRACTION
            uniform float _RefrDistort;
        #endif

        sampler2D _PlayerDepthTex;
        sampler2D _WorldDepthTex;
        float4x4 _PlayerMatrix;
        float4x4 _WorldMatrix;
        float4 playerCoords;
        float4 worldCoords;
        float _ShadowBias;
        float _ShadowStrength;
        float _ShadowFalloff;
        float _ShadowFalloffPower;
    
        struct appdata {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
        };

        struct v2f {
            float4 pos : SV_POSITION;
            
            #if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
                float4 ref : TEXCOORD0;
                float2 bumpuv0 : TEXCOORD1;
                float2 bumpuv1 : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
            #else
                float2 bumpuv0 : TEXCOORD0;
                float2 bumpuv1 : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            #endif
            
            float3 normal : TEXCOORD4;
            float4 playerCoords : TEXCOORD5;
            float4 worldCoords : TEXCOORD6;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.normal = mul((float3x3)unity_ObjectToWorld, SCALED_NORMAL);
            
            // scroll bump waves
            float4 temp;
            temp.xyzw = v.vertex.xzxz * _WaveScale4 / 1.0 + _WaveOffset;
            o.bumpuv0 = temp.xy;
            o.bumpuv1 = temp.wz;
            
            // object space view direction (will normalize per pixel)
            o.viewDir.xzy = ObjSpaceViewDir(v.vertex);
            
            #if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
            o.ref = ComputeScreenPos(o.pos);
            #endif
            
            float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
            o.playerCoords = mul(_PlayerMatrix, float4(worldPos, 1.0));
            o.worldCoords = mul(_WorldMatrix, float4(worldPos, 1.0));
            
            return o;
        }
        
        #if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
            sampler2D _ReflectionTex;
        #endif
        
        #if defined (WATER_REFLECTIVE) || defined (WATER_SIMPLE)
            sampler2D _ReflectiveColor;
        #endif
        
        #if defined (WATER_REFRACTIVE)
            sampler2D _Fresnel;
            sampler2D _RefractionTex;
            uniform float4 _RefrColor;
        #endif
        
        #if defined (WATER_SIMPLE)
            uniform float4 _HorizonColor;
        #endif
        
        sampler2D _BumpMap;

        half4 frag( v2f i ) : COLOR
        {
            i.viewDir = normalize(i.viewDir);
    
            // combine two scrolling bumpmaps into one
            half3 bump1 = UnpackNormal(tex2D( _BumpMap, i.bumpuv0 )).rgb;
            half3 bump2 = UnpackNormal(tex2D( _BumpMap, i.bumpuv1 )).rgb;
            half3 bump = (bump1 + bump2) * 0.5;
            
            // fresnel factor
            half fresnelFac = dot( i.viewDir, bump );
    
            // perturb reflection/refraction UVs by bumpmap, and lookup colors
            
            #if HAS_REFLECTION
                float4 uv1 = i.ref; uv1.xy += bump * _ReflDistort;
                half4 refl = tex2Dproj( _ReflectionTex, UNITY_PROJ_COORD(uv1) );
            #endif

            #if HAS_REFRACTION
                float4 uv2 = i.ref; uv2.xy -= bump * _RefrDistort;
                half4 refr = tex2Dproj( _RefractionTex, UNITY_PROJ_COORD(uv2) ) * _RefrColor;
            #endif
    
            // final color is between refracted and reflected based on fresnel    
            half4 color;
    
            #if defined(WATER_REFRACTIVE)
                half fresnel = tex2D( _Fresnel, float2(fresnelFac,fresnelFac) ).a;
                color = lerp( refr, refl, fresnel );
            #endif
    
            #if defined(WATER_REFLECTIVE)
                half4 water = tex2D( _ReflectiveColor, float2(fresnelFac,fresnelFac) );
                color.rgb = lerp( water.rgb, refl.rgb, water.a );
                color.a = refl.a * water.a;
            #endif
    
            #if defined(WATER_SIMPLE)
                half4 water = tex2D( _ReflectiveColor, float2(fresnelFac,fresnelFac) );
                color.rgb = lerp( water.rgb, _HorizonColor.rgb, water.a );
                color.a = _HorizonColor.a;
                //color.a = 1.0f;
            #endif
            
            if(i.playerCoords.x >= 0 && i.playerCoords.x < i.playerCoords.w
            && i.playerCoords.y >= 0 && i.playerCoords.y < i.playerCoords.w)
            {
                float3 normal = normalize(i.normal);
                
                float playerDepth = tex2Dproj(_PlayerDepthTex, i.playerCoords).r;
                float worldDepth = tex2Dproj(_WorldDepthTex, i.worldCoords).r;
                
#if defined(UNITY_REVERSED_Z)
                playerDepth = 1.0 - playerDepth;
                worldDepth = 1.0 - worldDepth;
#endif

                float shade = 1.0 - saturate(dot(normal, float3(0, 1, 0))) * _ShadowStrength;
                shade = lerp(1.0, shade, 1.0 - pow(saturate(worldDepth / _ShadowFalloff), _ShadowFalloffPower));
                bool isBehindPlayer = (i.playerCoords.z - _ShadowBias) > playerDepth;
                bool isTopLayer = i.worldCoords.z <= worldDepth + 0.005;
                float shadow = (isBehindPlayer && isTopLayer) ? shade : 1.0;
                color.rgb *= shadow;
            }
            
            return color;
        }
        ENDCG

    }
}

// -----------------------------------------------------------
//  Old cards

// three texture, cubemaps
Subshader {
    Tags { "WaterMode"="Simple" "RenderType"="Opaque" }
    Pass {
        Color (0.5,0.5,0.5,0.5)
        SetTexture [_MainTex] {
            Matrix [_WaveMatrix]
            combine texture * primary
        }
        SetTexture [_MainTex] {
            Matrix [_WaveMatrix2]
            combine texture * primary + previous
        }
        SetTexture [_ReflectiveColorCube] {
            combine texture +- previous, primary
            Matrix [_Reflection]
        }
    }
}

// dual texture, cubemaps
Subshader {
    Tags { "WaterMode"="Simple" "RenderType"="Opaque" }
    Pass {
        Color (0.5,0.5,0.5,0.5)
        SetTexture [_MainTex] {
            Matrix [_WaveMatrix]
            combine texture
        }
        SetTexture [_ReflectiveColorCube] {
            combine texture +- previous, primary
            Matrix [_Reflection]
        }
    }
}

// single texture
Subshader {
    Tags { "WaterMode"="Simple" "RenderType"="Opaque" }
    Pass {
        Color (0.5,0.5,0.5,0)
        SetTexture [_MainTex] {
            Matrix [_WaveMatrix]
            combine texture, primary
        }
    }
}


}
