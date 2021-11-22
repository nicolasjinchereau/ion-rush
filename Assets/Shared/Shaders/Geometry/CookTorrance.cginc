#define PI 3.1415926535897932384626433

inline float Roughness(float nh, float m)
{
	float nh_nh = nh * nh;
	float mm_nh_nh = m * m * nh_nh;
	float simp = (1 - nh_nh) / mm_nh_nh; // should actully be (nh_nh - 1)
	return exp(-simp) / (PI * mm_nh_nh * nh_nh);
}

inline float Fresnel_Schlick(float reflectance, float vh)
{
    return reflectance + (1.0 - reflectance) * pow(1 - vh, 5);
}

inline float4 CookTorrance(
    float3 albedo, float alpha, float3 normal, float specular,
    float gloss, float4 lightCol, float4 specCol, float RMS,
    float3 lightDir, float3 viewDir, float atten)
{
	float3 h = normalize(lightDir + viewDir);
	float nh = dot(normal, h);
	float nv = dot(normal, viewDir);
	float nl = dot(normal, lightDir);
	float vh = dot(viewDir, h);
    
    float nh2 = 2 * nh;
    float geomAtten = saturate(min((nh2 * nv) / vh, (nh2 * nl) / vh));
    
	float diff = saturate(nl);
    float fresnel = Fresnel_Schlick(1 - specular, vh); // 1-spec...why?
    float roughness = Roughness(nh, RMS);
	float spec = saturate(roughness * fresnel * gloss / nv);
    
	float4 lightColAtt = lightCol * geomAtten * atten;
	float4 finalSpecCol = specCol * spec;
    
    float4 c;
	c.rgb = (albedo * diff + finalSpecCol.rgb) * lightColAtt.rgb;
    c.a = alpha + lightColAtt.a * finalSpecCol.a;
	return c;
}

#define COOK_TORRANCE(surfOutput, lightCol, specCol, RMS, lightDir, viewDir, atten) \
    CookTorrance(surfOutput.Albedo, surfOutput.Alpha, surfOutput.Normal, surfOutput.Specular, surfOutput.Gloss, lightCol, specCol, RMS, lightDir, viewDir, atten)
