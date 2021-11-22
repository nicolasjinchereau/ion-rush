using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class Localizer
{
    static Localizer _instance = null;

    public static Localizer Instance
    {
        get
        {
            if(_instance == null) {
                _instance = new Localizer();
                _instance.Initialize();
            }

            return _instance;
        }
    }

    List<string> languages = new List<string>();
    Dictionary<string, List<string>> translations = new Dictionary<string, List<string>>();
    int currentLanguage = 0;

    public static string CurrentLanuageName {
        get { return Instance.languages[Instance.currentLanguage]; }
    }

    public static int CurrentLanuageIndex {
        get { return Instance.currentLanguage; }
    }

    void Initialize()
    {
        var content = Resources.Load<TextAsset>("localization").text;

        string[][] csv = CsvParser.Parse(content);

        languages.Clear();
        translations.Clear();

        for(int i = 1; i < csv[0].Length; ++i)
            languages.Add(csv[0][i]);

        for(int row = 1; row < csv.Length; ++row)
        {
            if(csv[row].Length == (1 + languages.Count) && !string.IsNullOrEmpty(csv[row][0]))
            {
                var key = csv[row][0];
                var terms = new List<string>();

                for(int c = 1; c < csv[row].Length; ++c)
                    terms.Add(csv[row][c]);

                if(translations.ContainsKey(key))
                    Debug.LogError(string.Format("Localization table contains duplicate key: '{0}'", key));
                
                translations[key] = terms;
            }
        }
    }

    public static string Get(string key, bool returnKeyIfMissing = false)
    {
        List<string> translations;

        if(string.IsNullOrEmpty(key) ||
           !Instance.translations.TryGetValue(key, out translations))
        {
            Debug.LogError("Invalid localization key: " + (key ?? "<empty>"));
            return returnKeyIfMissing ? key : null;
        }

        return translations[Instance.currentLanguage];
    }

    public static void SetLanguage(string language)
    {
        var index = Instance.languages.FindIndex(lang => string.Compare(lang, language, true) == 0);
        if(index == -1)
            throw new ArgumentException("Invalid language - must be one of (" + Instance.languages.Join(", ") + ")", "language");
        
        Instance.currentLanguage = index;
        Instance.UpdateTextElements();
    }

    public static bool IsCJK(char c)
    {
        int code = (int)c;

        return (code >= 0x4E00 && code <= 0x9FFF)
            || (code >= 0x3400 && code <= 0x4DBF)
            || (code >= 0x20000 && code <= 0x2A6DF)
            || (code >= 0x2A700 && code <= 0x2B73F);
    }

    public static string FontName { get { return "fonts/comic_bold_chinese.ttf"; } }
    public static string FontCharset { get { return CreateCharset(); } }
    public static string FontCapsCharset { get { return CreateCharset(true); } }

    public static string CreateCharset(bool capsOnly = false)
    {
        HashSet<char> charSet = new HashSet<char>();

        foreach(var kv in Instance.translations)
        {
            foreach(char c in kv.Value[CurrentLanuageIndex])
                charSet.Add(c);
        }

        var s1 = " 0123456789";
        for(int i = 0; i < s1.Length; ++i)
            charSet.Add(s1[i]);

        if(!capsOnly)
        {
            var s2 = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

            for(int i = 0, len = s2.Length; i < len; ++i)
                charSet.Add(s2[i]);
        }

        var sb = new StringBuilder(charSet.Count);

        foreach(char c in charSet)
        {
            if(!capsOnly || IsCJK(c) || char.ToUpper(c) == c || char.ToUpper(c) == char.ToLower(c))
                sb.Append(c);
        }

        return sb.ToString();
    }

    public void UpdateTextElements()
    {
        foreach(var text in Util.FindSceneObjects<LocalizedText>())
        {
            if(text.enabled)
                text.UpdateText();
        }
    }
}
