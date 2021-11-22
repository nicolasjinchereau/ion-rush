using UnityEngine;
using System.Collections;

public static class Curve
{
    public delegate float Function(float t);

    public static float Constant(float t)
    {
        return 1.0f;
    }
    
    public static float InLinear(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return t;
    }
    
    public static float OutLinear(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return 1.0f - t;
    }

    public static float InPower(float t, float p)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return Mathf.Pow(t, p);
    }

    public static float InPowerInv(float t, float p)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return 1.0f - Mathf.Pow(t, p);
    }

    public static float OutPower(float t, float p)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return Mathf.Pow(1.0f - t, p);
    }

    public static float OutPowerInv(float t, float p)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return 1.0f - Mathf.Pow(1.0f - t, p);
    }

    public static float InQuad(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return t * t;
    }
    
    public static float InQuadInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return 1 - (t * t);
    }
    
    public static float OutQuad(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        t = (1 - t);
        return t * t;
    }
    
    public static float OutQuadInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return t * (2 - t);
    }
    
    public static float InCube(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return t * t * t;
    }
    
    public static float InCubeInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return 1 - (t * t * t);
    }
    
    public static float OutCube(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float inv = 1 - t;
        return inv * inv * inv;
    }
    
    public static float OutCubeInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float inv = 1 - t;
        return 1 - (inv * inv * inv);
    }

    public static float InQuart(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t2 = t * t; // t ^ 2
        return t2 * t2;
    }
    
    public static float InQuartInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t2 = t * t; // t ^ 2
        return 1 - t2 * t2;
    }
    
    public static float OutQuart(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t_inv = 1 - t;
        float t2_inv = t_inv * t_inv; // t_inv ^ 2
        return t2_inv * t2_inv;
    }
    
    public static float OutQuartInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t_inv = 1 - t;
        float t2_inv = t_inv * t_inv; // t_inv ^ 2
        return 1 - t2_inv * t2_inv;
    }

    public static float InDec(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t2 = t * t; // t ^ 2
        float t4 = t2 * t2; // t ^ 4
        return t2 * t4 * t4;
    }
    
    public static float InDecInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t2 = t * t; // t ^ 2
        float t4 = t2 * t2; // t ^ 4
        return 1 - t2 * t4 * t4;
    }
    
    public static float OutDec(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t_inv = 1 - t;
        float t2 = t_inv * t_inv; // t_inv ^ 2
        float t4 = t2 * t2; // t_inv ^ 4
        return t2 * t4 * t4;
    }
    
    public static float OutDecInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t_inv = 1 - t;
        float t2 = t_inv * t_inv; // t_inv ^ 2
        float t4 = t2 * t2; // t_inv ^ 4
        return 1 - t2 * t4 * t4;
    }
    
    public static float InIcosa(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t2 = t * t; // t ^ 2
        float t4 = t2 * t2; // t ^ 4
        float t8 = t4 * t4; // t ^ 8
        return t8 * t8 * t4; // t ^ 20
    }
    
    public static float InIcosaInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t2 = t * t; // t ^ 2
        float t4 = t2 * t2; // t ^ 4
        float t8 = t4 * t4; // t ^ 8
        return 1- t8 * t8 * t4; // 1 - t ^ 20
    }
    
    public static float OutIcosa(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t_inv = (1 - t);
        float t_inv2 = t_inv * t_inv; // (1 - t) ^ 2
        float t_inv4 = t_inv2 * t_inv2; // (1 - t) ^ 4
        float t_inv8 = t_inv4 * t_inv4; // (1 - t) ^ 8
        return t_inv8 * t_inv8 * t_inv4; // (1 - t) ^ 20
    }
    
    public static float OutIcosaInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t_inv = (1 - t);
        float t_inv2 = t_inv * t_inv; // (1 - t) ^ 2
        float t_inv4 = t_inv2 * t_inv2; // (1 - t) ^ 4
        float t_inv8 = t_inv4 * t_inv4; // (1 - t) ^ 8
        return 1 - t_inv8 * t_inv8 * t_inv4; // (1 - t) ^ 20
    }
    
    public static float InTetraconta(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t2 = t * t; // (1 - t) ^ 2
        float t4 = t2 * t2; // (1 - t) ^ 4
        float t8 = t4 * t4; // (1 - t) ^ 8
        float t16 = t8 * t8; // (1 - t) ^ 16
        return t16 * t16 * t8; // (1 - t) ^ 40
    }
    
    public static float InTetracontaInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t2 = t * t; // (1 - t) ^ 2
        float t4 = t2 * t2; // (1 - t) ^ 4
        float t8 = t4 * t4; // (1 - t) ^ 8
        float t16 = t8 * t8; // (1 - t) ^ 16
        return 1 - t16 * t16 * t8; // (1 - t) ^ 40
    }
    
    public static float OutTetraconta(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t_inv = (1 - t);
        float t_inv2 = t_inv * t_inv; // (1 - t) ^ 2
        float t_inv4 = t_inv2 * t_inv2; // (1 - t) ^ 4
        float t_inv8 = t_inv4 * t_inv4; // (1 - t) ^ 8
        float t_inv16 = t_inv8 * t_inv8; // (1 - t) ^ 16
        return t_inv16 * t_inv16 * t_inv8; // (1 - t) ^ 40
    }
    
    public static float OutTetracontaInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t_inv = (1 - t);
        float t_inv2 = t_inv * t_inv; // (1 - t) ^ 2
        float t_inv4 = t_inv2 * t_inv2; // (1 - t) ^ 4
        float t_inv8 = t_inv4 * t_inv4; // (1 - t) ^ 8
        float t_inv16 = t_inv8 * t_inv8; // (1 - t) ^ 16
        return 1 - t_inv16 * t_inv16 * t_inv8; // (1 - t) ^ 40
    }
    
    public static float RoughStepIn(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return t + (t - (t * t * (3 - 2 * t)));
    }

    public static float RoughStepInSteep(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return t + (t - (t * t * t * (t * (6 * t - 15) + 10)));
    }

    public static float SmoothStepIn(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return t * t * (3 - (2 * t));
    }
    
    public static float SmoothStepOut(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return 1 - (t * t * (3 - (2 * t)));
    }
    
    public static float SmoothStepInOut(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return (t < 0.5f) ? (t * t * (3 - (2 * t))) : (1 - (t * t * (3 - (2 * t))));
    }
    
    public static float SmoothStepInSteep(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return t * t * t * (t * (6 * t - 15) + 10);
    }
    
    public static float SmoothStepOutSteep(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return 1 - (t * t * t * (t * (6 * t - 15) + 10));
    }
    
    public static float SmoothStepInOutSteep(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return (t < 0.5f) ? (t * t * t * (t * (6 * t - 15) + 10)) : (1 - (t * t * t * (t * (6 * t - 15) + 10)));
    }
    
    public static float InElastic(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float ts = t * t;
        float tc = t * t * t;
        return 33 * tc * ts - 106 * ts * ts + 126 * tc - 67 * ts + 15 * t;
    }
    
    public static float InElasticStrong(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float ts = t * t;
        float tc = t * t * t;
        return 56 * tc * ts - 175 * ts * ts + 200 * tc - 100 * ts + 20 * t;
    }
    
    public static float OutElastic(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float ts = t * t;
        float tc = t * t * t;
        return 1 - (33 * tc * ts - 106 * ts * ts + 126 * tc - 67 * ts + 15 * t);
    }
    
    public static float OutElasticStrong(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float ts = t * t;
        float tc = t * t * t;
        return 1 - (56 * tc * ts - 175 * ts * ts + 200 * tc - 100 * ts + 20 * t);
    }
    
    public static float ArcLinear(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return 1 - (2 * Mathf.Abs(t - 0.5f));
    }
    
    public static float ArcLinearInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return (2 * Mathf.Abs(t - 0.5f));
    }
    
    public static float ArcQuad(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = (2 * t - 1);
        return 1 - (x * x);
    }
    
    public static float ArcQuadInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = (2 * t - 1);
        return (x * x);
    }
    
    public static float ArcCube(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = (2 * t - 1);
        float xsq = x * x;
        return 1 - ((0.5f * xsq) + (0.5f * (xsq * xsq)));
    }
    
    public static float ArcCubeInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = (2 * t - 1);
        float xsq = x * x;
        return (0.5f * xsq) + (0.5f * (xsq * xsq));
    }
    
    public static float ArcQuart(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = (2 * t - 1);
        float xsq = x * x;
        return 1 - (xsq * xsq);
    }
    
    public static float ArcQuartInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = (2 * t - 1);
        float xsq = x * x;
        return (xsq * xsq);
    }
    
    public static float ArcOct(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = (2 * t - 1);
        float xsq = x * x;
        float xc = xsq * xsq;
        return 1 - (xc * xc);
    }
    
    public static float ArcOctInv(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = (2 * t - 1);
        float xsq = x * x;
        float xc = xsq * xsq;
        return (xc * xc);
    }
    
    public static float ArcQuadOutSharp(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t1 = t - 1;
        float x = (2 * t1 * t1 - 1);
        return 1 - (x * x);
    }
    
    public static float ArcQuadInSharp(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = (2 * t * t - 1);
        return 1 - (x * x);
    }
    
    public static float ArcCubeOutSharp(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t1 = t - 1;
        float x = (2 * t1 * t1 - 1);
        float xsq = x * x;
        return 1 - ((0.5f * xsq) + (0.5f * (xsq * xsq)));
    }
    
    public static float ArcCubeInSharp(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = (2 * t * t - 1);
        float xsq = x * x;
        return 1 - ((0.5f * xsq) + (0.5f * (xsq * xsq)));
    }
    
    public static float ArcQuartOutSharp(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float t1 = t - 1;
        float x = (2 * t1 * t1 - 1);
        float xsq = x * x;
        return 1 - (xsq * xsq);
    }
    
    public static float ArcQuartInSharp(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float x = (2 * t * t - 1);
        float xsq = x * x;
        return 1 - (xsq * xsq);
    }

    public static float BounceIn(float t)
    {
        float s = 7.5625f;
        float p = 2.75f;
        float l;
        
        if (t < (1f / p))
        {
            l = s * t * t;
        }
        else
        {
            if (t < (2f / p))
            {
                t -= (1.5f / p);
                l = s * t * t + 0.75f;
            }
            else
            {
                if (t < (2.5f / p))
                {
                    t -= (2.25f / p);
                    l = s * t * t + 0.9375f;
                }
                else
                {
                    t -= (2.625f / p);
                    l = s * t * t + 0.984375f;
                }
            }
        }
        return l;
    }

    // 0 => 1 => 0 => -1 => 0
    public static float Sine(float t)
    {
        //t = Mathf.Clamp(t, 0.0f, 1.0f);
        return Mathf.Sin(t * Mathf.PI * 2.0f);
    }

    // 1 => 0 => -1 => 0 => 1
    public static float Cos(float t)
    {
        //t = Mathf.Clamp(t, 0.0f, 1.0f);
        return Mathf.Cos(t * Mathf.PI * 2.0f);
    }

    // 0 => 1 => 0
    public static float ArcSinewave(float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return Mathf.Cos((t + 0.5f) * Mathf.PI * 2.0f) * 0.5f + 0.5f;
    }
};