using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public enum MusicState
{
    None,
    Starting,
    Playing,
    CrossFading,
    Stopping
}

struct QueuedSong
{
    public float time;
    public AudioClip song;
}

public class MusicMixer : MonoBehaviour
{
    public static MusicMixer that { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (!that)
        {
            var prefab = Resources.Load<GameObject>("MusicMixer");
            var go = Instantiate(prefab);
            DontDestroyOnLoad(go);
            that = go.GetComponent<MusicMixer>();
        }
    }

    public AudioMixer audioMixer;
    public AudioMixerGroup musicMixerGroup;
    public AudioSource current;
    public AudioSource next;
    public bool useQuadraticVolumeMapping = true;

    float _currentVolume = 0;
    float _nextVolume = 0;
    MusicState _state = MusicState.None;
    
    public MusicState State => _state;

    public static AudioMixer Mixer {
        get => that.audioMixer;
    }

    public static AudioMixerGroup MixerGroup {
        get => that.musicMixerGroup;
    }

    public static bool MixerMute
    {
        set {
            Mixer.SetFloat("musicMute", value ? -80 : 0);
        }

        get {
            float volume = 0;
            Mixer.GetFloat("musicMute", out volume);
            return volume < -40;
        }
    }

    static float _mixerVolume = 1.0f;
    public static float MixerVolume
    {
        set
        {
            _mixerVolume = value;
            var volume = Mathf.Log10(_mixerVolume * _mixerVolume) * 20.0f;
            volume = Mathf.Clamp(volume, -80.0f, 0.0f);
            Mixer.SetFloat("musicVolume", volume);
        }

        get => _mixerVolume;
    }

    public void Awake()
    {
        if (that)
        {
            if (current.playOnAwake && current.clip != null)
            {
                current.Stop();
                Play(current.clip, 0, UnmapVolume(current.volume), current.loop, 0);
            }

            Destroy(this.gameObject);
            return;
        }
        else
        {
            that = this;
            DontDestroyOnLoad(gameObject);

            if (current.playOnAwake && current.clip != null)
            {
                _state = MusicState.Playing;
            }
        }
    }
    
    const float CallbackAtEndOfSong = -1;

    public static void PlayWithLoopingSegment(AudioClip clip, float startTime, float volume, float fadeInDuration, float loopStart, float loopEnd, float loopFadeDuration)
    {
        Action callback = null;

        callback = () => {
            CrossFade(clip, loopStart, volume, false, loopFadeDuration, callback, loopEnd);
        };

        Play(clip, startTime, volume, false, fadeInDuration, callback, loopEnd);
    }

    public static Coroutine Play(AudioClip clip, float startTime, float volume, bool loop, float fadeDuration, Action callback = null, float callbackTime = CallbackAtEndOfSong)
    {
        that.StopAllCoroutines();
        if (callbackTime > 0 && callbackTime < fadeDuration) throw new Exception("'callbackTime' cannot be less than 'fadeDuration'");
        return that.StartCoroutine(that.PlayAudio(clip, 0, startTime, volume, loop, fadeDuration, callback, callbackTime));
    }

    public static Coroutine PlayDelayed(AudioClip clip, float delay, float startTime, float volume, bool loop, float fadeDuration, Action callback = null, float callbackTime = CallbackAtEndOfSong)
    {
        that.StopAllCoroutines();
        if (callbackTime > 0 && callbackTime < fadeDuration) throw new Exception("'callbackTime' cannot be less than 'fadeDuration'");
        return that.StartCoroutine(that.PlayAudio(clip, delay, startTime, volume, loop, fadeDuration, callback, callbackTime));
    }

    public static Coroutine CrossFade(AudioClip clip, float startTime, float volume, bool loop, float fadeDuration, Action callback = null, float callbackTime = CallbackAtEndOfSong)
    {
        that.StopAllCoroutines();
        if (callbackTime > 0 && callbackTime < fadeDuration) throw new Exception("'callbackTime' cannot be less than 'fadeDuration'");
        return that.StartCoroutine(that.CrossFadeAudio(clip, startTime, volume, loop, fadeDuration, callback, callbackTime));
    }

    public static Coroutine Stop(float fadeDuration = 0)
    {
        that.StopAllCoroutines();
        return that.StartCoroutine(that.StopAudio(fadeDuration));
    }

    public float CurrentVolume {
        private set { current.volume = MapVolume(_currentVolume = value); }
        get { return _currentVolume; }
    }

    public float NextVolume {
        private set { next.volume = MapVolume(_nextVolume = value); }
        get { return _nextVolume; }
    }

    static float duckVolume = 1.0f;
    static float duckSpeed = 0.0f;

    public static void BeginDucking(float targetVolume, float fadeDuration)
    {
        duckVolume = targetVolume;
        duckSpeed = (1.0f - targetVolume) / fadeDuration;
    }

    public static void EndDucking()
    {
        duckVolume = 1.0f;
    }

    private void Update()
    {
        if (MixerVolume > duckVolume)
        {
            MixerVolume = Mathf.Max(MixerVolume - Time.deltaTime * duckSpeed, duckVolume);
        }
        else if (MixerVolume < duckVolume)
        {
            MixerVolume = Mathf.Min(MixerVolume + Time.deltaTime * duckSpeed, duckVolume);
        }
    }

    float MapVolume(float volume)
    {
        return useQuadraticVolumeMapping ? (volume * volume) : volume;
    }

    float UnmapVolume(float volume)
    {
        return useQuadraticVolumeMapping ? (volume > 0.0f ? Mathf.Sqrt(volume) : 0.0f) : volume;
    }

    IEnumerator PlayAudio(AudioClip clip, float delay, float startTime, float volume, bool loop, float fadeDuration, Action callback, float callbackTime)
    {
        _state = MusicState.Starting;

        current.Stop();
        next.Stop();
        next.clip = null;

        current.clip = clip;
        current.loop = loop;
        current.time = startTime;

        if (delay > 0.001f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (fadeDuration > 0)
        {
            CurrentVolume = 0;
            current.Play();

            yield return StartCoroutine(Util.Blend(fadeDuration, t => {
                CurrentVolume = Mathf.Lerp(0, volume, t);
            }));
        }
        else
        {
            CurrentVolume = volume;
            current.Play();
        }

        _state = MusicState.Playing;

        if (callback != null)
        {
            if (callbackTime < 0)
                callbackTime = current.clip.length;

            float callbackDelay = callbackTime - current.time;
            yield return new WaitForSeconds(callbackDelay);
            callback.Invoke();
        }
    }

    IEnumerator CrossFadeAudio(AudioClip clip, float startTime, float volume, bool loop, float fadeDuration, Action callback, float callbackTime)
    {
        if (_state == MusicState.CrossFading)
        {
            Debug.LogError("MusicService: CrossFade already in progress - current song will be stopped.");
            current.Stop();
            SwapSources();
        }

        _state = MusicState.CrossFading;

        var currentStartVol = CurrentVolume;

        next.Stop();
        next.clip = clip;
        next.loop = loop;
        next.time = startTime;
        next.Play();

        if (fadeDuration > 0)
        {
            yield return StartCoroutine(Util.Blend(fadeDuration, t => {
                CurrentVolume = Mathf.Lerp(currentStartVol, 0, t);
                NextVolume = Mathf.Lerp(0, volume, t);
            }));
        }

        current.Stop();
        current.clip = null;

        SwapSources();

        _state = MusicState.Playing;

        if (callback != null)
        {
            if (callbackTime < 0)
                callbackTime = current.clip.length;

            float callbackDelay = callbackTime - current.time;
            yield return new WaitForSeconds(callbackDelay);
            callback.Invoke();
        }
    }

    IEnumerator StopAudio(float fadeDuration)
    {
        if (_state == MusicState.None)
            yield break;

        _state = MusicState.Stopping;

        if (fadeDuration > 0)
        {
            var currStartVolume = CurrentVolume;
            var nextStartVolume = NextVolume;

            yield return StartCoroutine(Util.Blend(fadeDuration, t => {
                CurrentVolume = Mathf.Lerp(currStartVolume, 0, t);
                NextVolume = Mathf.Lerp(nextStartVolume, 0, t);
            }));
        }

        current.Stop();
        current.clip = null;
        next.Stop();
        next.clip = null;
        
        _state = MusicState.None;
    }

    void SwapSources()
    {
        var tmp = current;
        current = next;
        next = tmp;

        var tmpVol = _currentVolume;
        _currentVolume = _nextVolume;
        _nextVolume = tmpVol;
    }
}
