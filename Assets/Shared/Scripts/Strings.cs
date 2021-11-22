using UnityEngine;
using System.Collections.Generic;
using System.IO;
using JsonFx;

public enum AppLanguage : int
{
    English = 0,
    French = 1,
    Spanish = 2,
    Chinese = 3,
}

public static partial class Strings
{
    static string[] _langNames = new string[]{
        "ENGLISH",
        "FRANÇAIS",
        "ESPAÑOL",
        "中国",
    };
    
    public static string fontName { get { return "fonts/comic_bold_chinese.ttf"; } }
    public static string fontCharset { get { return GetCharset(language); } }
    public static string fontCapsCharset { get { return GetCharset(language, true); } }
    
    private static Dictionary<string, string[]> _strings;
    public static Dictionary<string, string[]> strings
    {
        get {
            if(_strings == null)
            {
//#if UNITY_EDITOR
//                string txt = File.ReadAllText("Assets/Resources/text/string_table.json");
//#else
                string txt = Resources.Load<TextAsset>("string_table").text;
//#endif
                _strings = JsonReader.Deserialize<Dictionary<string, string[]>>(txt);
            }
            
            return _strings;
        }
    }
    
    public static string Get(string tag) {
        return strings[tag][(int)language];
    }

    public static string Get(string tag, AppLanguage lang) {
        return strings[tag][(int)lang];
    }

    public static bool TryGet(string tag, out string value)
    {
        string[] values;
        if(strings.TryGetValue(tag, out values)) {
            value = values[(int)language];
            return true;
        }

        value = null;
        return false;
    }

    public static AppLanguage language = AppLanguage.English;

    public static string LangString(AppLanguage lang) {
        return _langNames[(int)lang];
    }

    public static bool IsCJK(char c)
    {
        int code = (int)c;

        return (code >= 0x4E00 && code <= 0x9FFF)
            || (code >= 0x3400 && code <= 0x4DBF)
            || (code >= 0x20000 && code <= 0x2A6DF)
            || (code >= 0x2A700 && code <= 0x2B73F);
    }

    public static string GetCharset(AppLanguage lang, bool capsOnly = false)
    {
        HashSet<char> charSet = new HashSet<char>();

        foreach(string ln in _langNames)
        {
            for(int i = 0, len = ln.Length; i < len; ++i)
                charSet.Add(ln[i]);
        }
        
        foreach(var kv in strings)
        {
            foreach(char c in kv.Value[(int)lang])
                charSet.Add(c);
        }
        
        var s1 = " 0123456789";
        for(int i = 0, len = s1.Length; i < len; ++i)
            charSet.Add(s1[i]);

        if(!capsOnly)
        {
            var s2 = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

            for(int i = 0, len = s2.Length; i < len; ++i)
                charSet.Add(s2[i]);
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder(charSet.Count);

        foreach(char c in charSet)
        {
            if(!capsOnly || IsCJK(c) || char.ToUpper(c) == c || char.ToUpper(c) == char.ToLower(c))
                sb.Append(c);
        }
        
        return sb.ToString();
    }

    public static AppLanguage defaultLanguage
    {
        get {
            switch(Application.systemLanguage)
            {
                default:
                    goto case SystemLanguage.English;

                case SystemLanguage.English:
                    return AppLanguage.English;

                case SystemLanguage.French:
                    return AppLanguage.French;

                case SystemLanguage.Spanish:
                    return AppLanguage.Spanish;

                case SystemLanguage.Chinese:
                    return AppLanguage.Chinese;
            }
        }
    }
}
