using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JsonFx;
using UnityEngine.Audio;
using System.Threading;

public static class Util
{
    private static AndroidJavaObject _currentActivity = null;
    public static AndroidJavaObject currentActivity {
        get {
            if(_currentActivity == null)
            {
                var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                _currentActivity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            }
            
            return _currentActivity;
        }
    }
    
    public static T[] EnumValues<T>() {
        Type type = typeof(T);
        if(!type.IsEnum)
            throw new Exception("Invalid type. 'T' must be an enum");
        
        return (T[])Enum.GetValues(type);
    }

    static List<GameObject> rootGameObjects = new List<GameObject>(128);

    public static T GetComponentInScene<T>() where T : Component
    {
        T ret = null;

        rootGameObjects.Clear();
        SceneManager.GetActiveScene().GetRootGameObjects(rootGameObjects);

        foreach(var go in rootGameObjects) {
            ret = go.GetComponentInChildren<T>(true);
            if(ret) break;
        }

        return ret;
    }

    public static List<T> GetComponentsInScene<T>() where T : Component
    {
        var ret = new List<T>();
        
        rootGameObjects.Clear();
        SceneManager.GetActiveScene().GetRootGameObjects(rootGameObjects);

        foreach(var go in rootGameObjects) {
            ret.AddRange(go.GetComponentsInChildren<T>(true));
        }

        return ret;
    }

    public static int EnumCount<T>() {
        return Enum.GetValues(typeof(EquipmentType)).Length;
    }
    
    public static int levelIndex {
        get { return SceneManager.GetActiveScene().buildIndex - 1; }
    }
    
    ///<summary>"Level" + (index + 1).ToString("D2")</summary>
    public static string LevelName(int index) {
        return "Level" + (index + 1).ToString("D2");
    }
    
    private static string camelCaseToTitleCase(string input)
    {
        if(input.Length == 0)
            return "";

        string ret = char.ToUpper(input[0]).ToString();

        for(int i = 1; i < input.Length; ++i)
        {
            char c = input[i];

            if(char.IsUpper(c))
                ret += " ";

            ret += c.ToString();
        }

        return ret;
    }

    public static void Resize<T>(this List<T> list, int sz, T c = default(T))
    {
        int count = list.Count;

        if(sz < count)
        {
            list.RemoveRange(sz, count - sz);
        }
        else if(sz > count)
        {
            for(int i = 0, j = sz - count; i < j; ++i)
                list.Add(c);
        }
    }

    public static float signedRandomValue {
        get { return UnityEngine.Random.value * 2.0f - 1.0f; }
    }
    
    public static float Snap(float x, float span)
    {
        return (float)((int)((x / span) + (x < 0 ? -0.5f : 0.5f))) * span;
    }

    public static float Clamp(float value, float min, float max) {
        return (value < min) ? min : (value > max) ? max : value;
    }

    public static double Clamp(double value, double min, double max) {
        return (value < min) ? min : (value > max) ? max : value;
    }

    public static float NormalizedClamp(float val, float min, float max) {
        float range = max - min;
        return range != 0.0f ? (Clamp(val, min, max) - min) / range : 0.0f;
    }

    public static double NormalizedClamp(double val, double min, double max) {
        double range = max - min;
        return range != 0.0 ? (Clamp(val, min, max) - min) / range : 0.0;
    }
    
    public static Texture2D LoadTexture(string path) {
        return Resources.Load(path, typeof(Texture2D)) as Texture2D;
    }
    
    public static bool IsValidEmailAddress(string input)
    {
        if (String.IsNullOrEmpty(input))
            return false;

        try {
            return Regex.IsMatch(input,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase);
        }
        catch (System.Exception ex) {
            Debug.Log(ex.Message);
        }

        return false;
    }

    public static string ToJsonString(object obj, bool pretty)
    {
        JsonWriterSettings settings = new JsonWriterSettings() {
            PrettyPrint = pretty,
        };
        
        return JsonWriter.Serialize(obj, settings);
    }
    
    public static string documentsPath
    {
        get
        {
            string path = "";

#if UNITY_IOS && !UNITY_EDITOR
            // Your game has read+write access to /var/mobile/Applications/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX/Documents
            // Application.dataPath returns
            // /var/mobile/Applications/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX/myappname.app/Data
            // Strip "/Data" from path
            //path = Application.dataPath.Substring (0, Application.dataPath.Length - 5);

            // Strip application name
            //path = path.Substring(0, path.LastIndexOf('/')) + "/Documents";

            path = Application.persistentDataPath;
#elif UNITY_ANDROID && !UNITY_EDITOR
            path = Application.persistentDataPath;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/Documents/IonRush";
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/IonRush";
#endif
            return path;
        }
    }

    public static GameObject AddChild(this GameObject to, GameObject go) {
        go.transform.parent = to.transform;
        return go;
    }

    public static GameObject RemoveChild(this GameObject from, GameObject go) {
        go.transform.parent = null;
        return go;
    }

    public static GameObject RemoveChild(this GameObject from, string path)
    {
        GameObject ret = null;

        Transform xf = from.transform.Find(path);
        if(xf != null)
        {
            ret = xf.gameObject;
            xf.parent = null;
        }

        return ret;
    }

    public static AudioSource PlayClip(AudioClip clip, Vector3 pos, float volume = 1.0f)
    {
        GameObject go = new GameObject();
        go.transform.position = pos;
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1;
        src.loop = false;
        src.volume = volume;
        src.Play();
        GameObject.Destroy(go, clip.length + 0.25f);
        return src;
    }

    public static AudioSource PlayClip(AudioClip clip, float volume = 1.0f) {
        return PlayClip(clip, null, volume);
    }

    public static AudioSource PlayClip(AudioClip clip, AudioMixerGroup mixerGroup, float volume = 1.0f)
    {
        GameObject go = new GameObject();
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 0;
        src.bypassEffects = true;
        src.bypassListenerEffects = true;
        src.bypassReverbZones = true;
        src.ignoreListenerVolume = true;
        src.loop = false;
        src.spatialize = false;
        src.volume = volume;
        src.outputAudioMixerGroup = mixerGroup;
        src.Play();
        GameObject.Destroy(go, clip.length + 0.25f);
        return src;
    }

    public static List<T> FindSceneObjects<T>()
        where T : Component
    {
        var objs = Resources.FindObjectsOfTypeAll<T>();
        var ret = new List<T>(objs.Length);

        foreach(var obj in objs)
        {
#if UNITY_EDITOR
            if(!UnityEditor.EditorUtility.IsPersistent(obj.transform.root.gameObject))
                ret.Add(obj);
#else
            ret.Add(obj);
#endif
        }

        return ret;
    }

    public static IEnumerator Blend(float duration, Action<float> tick)
    {
        tick(0.0f);

        float speed = 1.0f / duration;
        float t = 0;

        while (true)
        {
            yield return null;

            t += Time.deltaTime * speed;

            if (t >= 1.0f)
                break;

            tick(t);
        }

        tick(1.0f);
    }

    public static IEnumerator Blend(float duration, float from, float to, Action<float> tick)
    {
        return Blend(duration, t => tick(Mathf.Lerp(from, to, t)));
    }

    public static IEnumerator Blend(float duration, Action<float> tick, Action after)
    {
        yield return Blend(duration, tick);
        after();
    }

    public static IEnumerator InvokeDelayed(float delay, Action action)
    {
        if (delay >= float.Epsilon)
            yield return new WaitForSeconds(delay);

        action();
    }

    public static IEnumerator InvokeRepeating(Action action) {
        return InvokeRepeating(t => action());
    }

    public static IEnumerator InvokeRepeating(Action<float> action)
    {
        var start = Time.time;

        while (true)
        {
            action(Time.time - start);
            yield return null;
        }
    }

    public static IEnumerator InvokeRepeating(float duration, Action action) {
        return InvokeRepeating(duration, t => action());
    }

    public static IEnumerator InvokeRepeating(float duration, Action<float> action)
    {
        var start = Time.time;
        var end = Time.time + duration;

        while (Time.time <= end)
        {
            action(Time.time - start);
            yield return null;
        }
    }

    public static IEnumerator InvokeRepeating(float delayMin, float delayMax, Action action) {
        return InvokeRepeating(delayMin, delayMax, t => action());
    }

    public static IEnumerator InvokeRepeating(float delayMin, float delayMax, Action<float> action)
    {
        var start = Time.time;

        while (true)
        {
            action(Time.time - start);
            yield return new WaitForSeconds(UnityEngine.Random.Range(delayMin, delayMax));
        }
    }

    public static IEnumerator InvokeRepeating(float duration, float delayMin, float delayMax, Action action) {
        return InvokeRepeating(duration, delayMin, delayMax, t => action());
    }

    public static IEnumerator InvokeRepeating(float duration, float delayMin, float delayMax, Action<float> action)
    {
        var start = Time.time;
        var end = Time.time + duration;

        while (Time.time <= end)
        {
            action(Time.time - start);
            yield return new WaitForSeconds(UnityEngine.Random.Range(delayMin, delayMax));
        }
    }

    public static IEnumerator InvokeNTimes(uint count, Action action)
    {
        for(uint i = 0; i != count; ++i)
        {
            action();
            yield return null;
        }
    }

    public static IEnumerator InvokeNTimes(uint count, float delayMin, float delayMax, Action action)
    {
        for (uint i = 0; i != count; ++i)
        {
            action();
            yield return new WaitForSeconds(UnityEngine.Random.Range(delayMin, delayMax));
        }
    }

    public static IEnumerator Orbit(Transform transform, Vector3 point, float duration, float degrees)
    {
        var rotationSpeed = degrees / duration;

        return Util.InvokeRepeating(duration, () => {
            transform.RotateAround(point, Vector3.up, rotationSpeed * Time.deltaTime);
        });
    }

    public static IEnumerator Orbit(Transform transform, float distance, float duration, float degrees) {
        var point = transform.position + transform.forward * distance;
        return Orbit(transform, point, duration, degrees);
    }

    public static IEnumerator Orbit(Transform transform, Vector3 point, float duration, float degrees, Curve.Function curve)
    {
        var startPos = transform.position;
        var startRot = transform.rotation;

        return Blend(duration, t => {
            transform.position = startPos;
            transform.rotation = startRot;
            transform.RotateAround(point, Vector3.up, degrees * curve(t));
        });
    }

    public const float InchesToCentimeters = 2.54f;
    
#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject _displayMetrics = null;
    public static AndroidJavaObject displayMetrics {
        get {
            if(_displayMetrics == null)
            {
                var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
                var windowManager = activity.Call<AndroidJavaObject>("getWindowManager");
                var defaultDisplay = windowManager.Call<AndroidJavaObject>("getDefaultDisplay");
                _displayMetrics = new AndroidJavaObject("android.util.DisplayMetrics");
                defaultDisplay.Call("getMetrics", _displayMetrics);
            }
            
            return _displayMetrics;
        }
    }
#endif

    public static float dpi {
        get {
#if UNITY_ANDROID && !UNITY_EDITOR
            float xdpi = displayMetrics.Get<float>("xdpi");
            float ydpi = displayMetrics.Get<float>("ydpi");
            return (xdpi + ydpi) * 0.5f;
#else
            return Screen.dpi;
#endif
        }
    }
    
    /// <summary>dots per centimeter</summary>
    public static float dpcm {
        get { return dpi / 2.54f; }
    }

    public static int CalculateTotalCoins(int coinsCollected, int gearsCollected, int batteriesRemaining, bool expertMode)
    {
        int total = 0;
        
        total += coinsCollected;
        total += gearsCollected * Defaults.CoinsPerGear;
        total += batteriesRemaining * Defaults.CoinsPerBattery;
        total *= expertMode ? Defaults.CoinExpertMultiplier : 1;
        
        return total;
    }
}
