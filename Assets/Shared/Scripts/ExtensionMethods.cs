using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Reflection;

public static class ExtensionMethods
{
    public static Color WithAlpha(this Color c, float a) {
        c.a = a;
        return c;
    }
    
    public static bool IsImplicitlyConvertibleTo(this Type sourceType, Type targetType)
    {
        return targetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(mi => mi.Name == "op_Implicit" && mi.ReturnType == targetType)
        .Any(mi => {
            ParameterInfo pi = mi.GetParameters().FirstOrDefault();
            return pi != null && pi.ParameterType == sourceType;
        });
    }
    
    public static object CastTo(this Type sourceType, Type targetType, object val)
    {
        var conv = targetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(mi => mi.Name == "op_Implicit" && mi.ReturnType == targetType)
        .FirstOrDefault(mi => {
            ParameterInfo pi = mi.GetParameters().FirstOrDefault();
            return pi != null && pi.ParameterType == sourceType;
        });
        
        return conv != null ? conv.Invoke(null, new object[]{ val }) : null;
    }
}

public static class StringBuilderExtensions
{
    public static void Clear(this StringBuilder sb) {
        sb.Length = 0;
    }
    
    public static void ShrinkToFit(this StringBuilder sb) {
        sb.Capacity = sb.Length;
    }
}

public static class IEnumerableExtensions
{
    public static string Join(this IEnumerable<string> source, string sep)
    {
        StringBuilder sb = new StringBuilder();

        var en = source.GetEnumerator();
        if(en.MoveNext())
            sb.Append(en.Current);
        
        while(en.MoveNext())
            sb.Append(sep).Append(en.Current);
        
        return sb.ToString();
    }
    
    // public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable source) {
    //     foreach(var o in source)
    //         yield return (TSource)o;
    // }
}

//public static class WWWExtensions
//{
//    public static bool Succeeded(this WWW www) {
//        return www.isDone && string.IsNullOrEmpty(www.error);
//    }
//}

public static class DateExtensions
{
    public static DateTime Round(this DateTime date, TimeSpan span)
    {
        long ticks = (date.Ticks + (span.Ticks / 2) + 1) / span.Ticks;
        return new DateTime(ticks * span.Ticks);
    }

    public static DateTime Floor(this DateTime date, TimeSpan span)
    {
        long ticks = (date.Ticks / span.Ticks);
        return new DateTime(ticks * span.Ticks);
    }

    public static DateTime Ceil(this DateTime date, TimeSpan span)
    {
        long ticks = (date.Ticks + span.Ticks - 1) / span.Ticks;
        return new DateTime(ticks * span.Ticks);
    }
}

public static class TransformExtensions
{
    public static void SetPositionAndRotation(this Transform transform, Transform other)
    {
        transform.SetPositionAndRotation(other.position, other.rotation);
    }
}

public static class QuaternionExtensions
{
    public static Quaternion Inverse(this Quaternion quaternion) {
        return Quaternion.Inverse(quaternion);
    }
}
