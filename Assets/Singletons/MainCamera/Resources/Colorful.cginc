/* Utils */
float luminance(float3 color)
{
    return dot(color, float3(0.30, 0.59, 0.11));
}

float rot(float value, float low, float hi)
{
    if (value < low)        value += hi;
    else if (value > hi)    value -= hi;
    return value;
}

float rot10(float value)
{
    return rot(value, 0.0, 1.0);
}

float4 pixelate(sampler2D tex, float2 uv, float scale)
{
    float ds = 1.0 / scale;
    float2 coord = float2(ds * ceil(uv.x / ds), ds * ceil(uv.y / ds));
    return float4(tex2D(tex, coord).xyzw);
}

float simpleNoise(float x, float y, float seed, float phase)
{
    float n = x * y * phase * seed;
    return fmod(n, 13) * fmod(n, 123);
}

/* Color conversion */
float3 HSVtoRGB(float3 hsv)
{
    float H = hsv.x * 6.0;
    float R = abs(H - 3.0) - 1.0;
    float G = 2 - abs(H - 2.0);
    float B = 2 - abs(H - 4.0);
    float3 hue = saturate(float3(R, G, B));
    return ((hue - 1) * hsv.y + 1) * hsv.z;
}

float3 RGBtoHSV(float3 rgb)
{
    float3 hsv = float3(0.0, 0.0, 0.0);

    hsv.z = max(rgb.r, max(rgb.g, rgb.b));
    float cMin = min(rgb.r, min(rgb.g, rgb.b));
    float C = hsv.z - cMin;

    if (C != 0)
    {
        hsv.y = C / hsv.z;
        float3 delta = (hsv.z - rgb) / C;
        delta.rgb -= delta.brg;
        delta.rg += float2(2.0, 4.0);

        if (rgb.r >= hsv.z)            hsv.x = delta.b;
        else if (rgb.g >= hsv.z)    hsv.x = delta.r;
        else                        hsv.x = delta.g;

        hsv.x = frac(hsv.x / 6);
    }

    return hsv;
}
