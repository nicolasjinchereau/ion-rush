Shader "Custom/Hologram"
{
    Properties
    {
        _MainTex ("Heightmap Texture (R)", 2D) = "white" {}
        _Height ("Height", Range(0.0, 1.0)) = 1.0
        _Color("Main Color", Color) = (1, 1, 1, 1)
        _LineColor("Line Color", Color) = (0.3, 0.3, 0.3, 1)
        _CapColor("Cap Color", Color) = (1, 0, 0, 1)
        _CapLineColor("Cap Line Color", Color) = (0.3, 0, 0, 1)
        _Split("Split", Range(0.0, 1.0)) = 0.5
        _Clip("Clip", Range(0.0, 1.0)) = 0.499
        _LineOffset("Line Offset", Range(-0.1, 0.1)) = 0
        _LineScale("Line Scale", Range(0.0, 100.0)) = 33.333
        _AmbientLight("Ambient Light", Range(0.0, 1.0)) = 0.3
        _StaticBlockCountX("Block Count X", Float) = 2
        _StaticBlockCountY("Block Count Y", Float) = 2
    }
    
    CGINCLUDE
    #include "UnityCG.cginc"

    struct v2f
    {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
        float3 normal : TEXCOORD2;
        float height : TEXCOORD3;
    };
    
    sampler2D _MainTex;
    float4 _MainTex_ST;
    float _Height;
    float4 _Color;
    float4 _LineColor;
    float4 _CapColor;
    float4 _CapLineColor;
    float _Split;
    float _Clip;
    float _LineOffset;
    float _LineScale;
    float _AmbientLight;
    float _StaticBlockCountX;
    float _StaticBlockCountY;

    v2f vert_common(appdata_base v, float clampHeight)
    {
        v2f o;
        o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

        float offset = 1.0 / 128.0;
        float offset2 = offset * 0.7071068;
        
        float4 tex = tex2Dlod(_MainTex, float4(o.uv.xy, 0, 0));
        float4 texL = tex2Dlod(_MainTex, float4(o.uv.x - offset, o.uv.y, 0, 0));
        float4 texR = tex2Dlod(_MainTex, float4(o.uv.x + offset, o.uv.y, 0, 0));
        float4 texT = tex2Dlod(_MainTex, float4(o.uv.x, o.uv.y + offset, 0, 0));
        float4 texB = tex2Dlod(_MainTex, float4(o.uv.x, o.uv.y - offset, 0, 0));
        float4 texTL = tex2Dlod(_MainTex, float4(o.uv.x - offset2, o.uv.y + offset2, 0, 0));
        float4 texTR = tex2Dlod(_MainTex, float4(o.uv.x + offset2, o.uv.y + offset2, 0, 0));
        float4 texBL = tex2Dlod(_MainTex, float4(o.uv.x - offset2, o.uv.y - offset2, 0, 0));
        float4 texBR = tex2Dlod(_MainTex, float4(o.uv.x + offset2, o.uv.y - offset2, 0, 0));

        tex = (tex + texL + texR + texT + texB + texTL + texTR + texBL + texBR) / 9.0;

        o.height = min(tex.r * _Height, _Clip * _Height);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex + float4(0, 0, min(o.height, clampHeight), 0));
        
        float3 worldPosLeft = mul(unity_ObjectToWorld, v.vertex + float4(-offset, 0, texL.r * _Height, 0));
        float3 worldPosRight = mul(unity_ObjectToWorld, v.vertex + float4(offset, 0, texR.r * _Height, 0));
        float3 worldPosUp = mul(unity_ObjectToWorld, v.vertex + float4(0, -offset, texT.r * _Height, 0));
        float3 worldPosDown = mul(unity_ObjectToWorld, v.vertex + float4(0, offset, texB.r * _Height, 0));

        float3 vx = worldPosRight - worldPosLeft;
        float3 vy = worldPosUp - worldPosDown;
        
        o.normal = normalize(cross(vx, vy));
        o.pos = mul(unity_MatrixVP, float4(o.worldPos, 1));

        return o;
    }

    float rand(float3 co)
    {
        return frac((sin(dot(co.xyz, float3(12.345 * _Time.w, 67.890 * _Time.w, 43.625 * _Time.w))) * 12345.67890 + _Time.w));
    }

    float4 getStatic(float4 color, float3 uvw)
    {
        float blockSizeX = 1.0 / _StaticBlockCountX;
        float blockSizeY = 1.0 / _StaticBlockCountY;

        float x = int(uvw.x / blockSizeX) * blockSizeX;
        float y = int(uvw.y / blockSizeX) * blockSizeX;
        float z = int(uvw.z / blockSizeX) * blockSizeX;

        float noise = rand(float3(x, y, z));
        return float4(lerp(color.rgb, color.rgb * 0.5f, noise), color.a);
    }

    float arcPow(float a, float b, float t, float p) {
        float x = (clamp(t, a, b) - a) / (b - a);
        return 1.0 - pow((2.0 * x - 1.0), p);
    }

    ENDCG

    SubShader
    {
        Tags { "Queue" = "Transparent" }
        ZWrite off
        ZTest off
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            v2f vert(appdata_base v) {
                return vert_common(v, _Split * _Height);
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 mainCol = getStatic(_Color, i.worldPos);
                
                float spacing = 35.0f;

                float lines = arcPow(0.94, 1.0, frac((i.worldPos.y + _LineOffset) * _LineScale), 4);
                lines = max(lines, arcPow(0.94, 1.0, frac((i.worldPos.x + _LineOffset) * _LineScale), 4));
                lines = max(lines, arcPow(0.94, 1.0, frac((i.worldPos.z + _LineOffset) * _LineScale), 4));
                
                float3 normal = normalize(i.normal);

                float4 col = lerp(mainCol, float4(_LineColor.rgb, _LineColor.a * mainCol.a), lines);

                float3 lightDir = normalize(_WorldSpaceLightPos0);
                col.rgb *= lerp(_AmbientLight, 1.0, saturate(dot(normal, lightDir)));
                return col;
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata_base v) {
                return vert_common(v, 1.0);
            }

            float4 frag(v2f i) : SV_Target
            {
                clip(i.height - _Split * (_Height + 0.001));

                float4 mainCol = getStatic(_CapColor, i.worldPos);

                float3 normal = normalize(i.normal);

                float lines = arcPow(0.94, 1.0, frac((i.worldPos.y + _LineOffset) * _LineScale), 4);
                lines = max(lines, arcPow(0.94, 1.0, frac((i.worldPos.x + _LineOffset) * _LineScale), 4));
                lines = max(lines, arcPow(0.94, 1.0, frac((i.worldPos.z + _LineOffset) * _LineScale), 4));

                float4 col = lerp(mainCol, float4(_CapLineColor.rgb, _CapLineColor.a * mainCol.a), lines);

                float3 lightDir = normalize(_WorldSpaceLightPos0);
                col.rgb *= lerp(_AmbientLight, 1.0, saturate(dot(normal, lightDir)));

                return col;
            }
            ENDCG
        }

    }
}
