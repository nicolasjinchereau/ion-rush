
inline float GetPlayerShadowAtten(
    float4 playerCoords, float4 worldCoords, float atten,
    sampler2D playerDepthTex, sampler2D worldDepthTex, float4 shadowParams)
{
    float ret = 1;

    if(playerCoords.x >= 0 && playerCoords.x < playerCoords.w
    && playerCoords.y >= 0 && playerCoords.y < playerCoords.w)
    {
        float playerDepth = tex2Dproj(playerDepthTex, playerCoords).r;
        float worldDepth = tex2Dproj(worldDepthTex, worldCoords).r;

#if defined(UNITY_REVERSED_Z)
        playerDepth = 1.0 - playerDepth;
        worldDepth = 1.0 - worldDepth;
#endif

        if((playerCoords.z - shadowParams.x > playerDepth) && (worldCoords.z - 0.005 <= worldDepth))
        {
            float shade = 1.0 - atten * shadowParams.y;
            ret = lerp(1.0, shade, 1.0 - pow(saturate(worldDepth / shadowParams.z), shadowParams.w));
        }
    }

    return ret;
}

#if defined(PLAYER_SHADOW_ON)
  #define PLAYER_SHADOW_UNIFORMS \
  sampler2D _PlayerDepthTex; \
  sampler2D _WorldDepthTex; \
  float4x4 _PlayerMatrix; \
  float4x4 _WorldMatrix; \
  float4 _PlayerShadowParams; // bias, strength, falloff, falloff-power

  #define PLAYER_SHADOW_V2S \
    float4 playerCoords; \
    float4 worldCoords; \
    float playerShadowAtten;

  #define PLAYER_SHADOW_SURFACE_OUTPUT \
    float4 playerCoords; \
    float4 worldCoords; \
    float playerShadowAtten;

  #define PLAYER_SHADOW_ATTEN(v2f) \
    GetPlayerShadowAtten(v2f.playerCoords, v2f.worldCoords, v2f.playerShadowAtten, _PlayerDepthTex, _WorldDepthTex, _PlayerShadowParams)

  #define PLAYER_SHADOW_VERT_TO_FRAG(vertex, normal, output) \
    float3 __playerShadow_worldPos = mul(unity_ObjectToWorld, float4(vertex.xyz, 1.0)).xyz; \
    float3 __playerShadow_worldNormal = normalize(mul(float4(normal, 0.0), unity_WorldToObject).xyz); \
    output.playerCoords = mul(_PlayerMatrix, float4(__playerShadow_worldPos, 1.0)); \
    output.worldCoords = mul(_WorldMatrix, float4(__playerShadow_worldPos, 1.0)); \
    output.playerShadowAtten = saturate(__playerShadow_worldNormal.y);

  #define PLAYER_SHADOW_SURFACE_INOUT(input, output) \
    output.playerCoords = input.playerCoords; \
    output.worldCoords = input.worldCoords; \
    output.playerShadowAtten = input.playerShadowAtten;
#else
  #define PLAYER_SHADOW_UNIFORMS
  #define PLAYER_SHADOW_V2S
  #define PLAYER_SHADOW_SURFACE_OUTPUT
  #define PLAYER_SHADOW_ATTEN(v2f) 1
  #define PLAYER_SHADOW_VERT_TO_FRAG(worldPos, normal, output)
  #define PLAYER_SHADOW_SURFACE_INOUT(input, output)
#endif

struct SurfaceOutputPlayerShadow
{
    float3 Albedo;
    float3 Normal;
    float3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
    PLAYER_SHADOW_SURFACE_OUTPUT
};
