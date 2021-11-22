using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Linq;

public class GameController : MonoBehaviour
{
    public static GameController that {
        get; private set;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateInstance()
    {
        if(that == null) {
            var go = new GameObject("Game");
            go.hideFlags = HideFlags.NotEditable;
            DontDestroyOnLoad(go);
            that = go.AddComponent<GameController>();
        }
    }

    private static bool _didStart = false;

    private bool _inTransition = false;
    private bool isPaused = false;
    public GameState _state = null;
    
    public static GameState state {
        get { return that._state; }
        set { that._state = value; }
    }

    public static T GetStateAs<T>() where T : GameState {
        return that._state as T;
    }

    //******************************************************
    
    void Awake()
    {
        Shader.WarmupAllShaders();
        StartCoroutine(_doStartup());
    }
    
    void Start()
    {
        SharedSounds.MixerMute = !Profile.SoundEffectsEnabled;
        MusicMixer.MixerMute = !Profile.MusicEnabled;
    }

    GameState FindDefaultState() {
        return Util.GetComponentsInScene<GameState>().Find(s => s.isDefaultState);
    }

    IEnumerator _doStartup()
    {
        _state = FindDefaultState();
        if(_state == null)
        {
            Debug.LogError("No default state was found in this level.");
            //Debug.Break();
            yield break;
        }
        
        object[] args = new object[0];

        _inTransition = true;

        _state.DoPreStart(args);

        yield return null;

        _state.DoStartup(args);
        _state.enabled = true;
        _inTransition = false;
        
        yield return null;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void OnApplicationPause(bool didPause)
    {
        if(didPause)
        {
            Profile.Save();
            isPaused = true;
        }
        else
        {
            if(isPaused)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                isPaused = false;
            }
        }
    }
    
    void OnApplicationQuit()
    {
        if(that._state != null)
        {
            that._inTransition = true;
            that._state.enabled = false;
            that._state.DoShutdown();
            that._inTransition = false;
        }

        Profile.Save();
    }

    //******************************************************

    public static void SetLevel(int levelIndex)
    {
        if (levelIndex >= 0)
            SetState(Util.LevelName(levelIndex), string.Empty);
        else
            SetState("Menu", string.Empty);
    }

    public static void SetState(GameState stateObject, params object[] args) {
        that._setState(stateObject, args);
    }

    public static void SetState(string levelName, string stateName, params object[] args) {
        that.StartCoroutine(that._setState(levelName, stateName, args));
    }

    void _setState(GameState stateObject, params object[] args)
    {
        if (_inTransition)
        {
            Debug.LogWarning("State Change Failed(" + stateObject.GetType().Name + "). State transition already in progress.");
            return;
        }

        _inTransition = true;

        _state.enabled = false;
        _state.DoShutdown();

        _state = stateObject;

        _state.DoPreStart(args);
        _state.DoStartup(args);
        _state.enabled = true;

        _inTransition = false;
    }

    IEnumerator _setState(string levelName, string stateName, params object[] args)
    {
        if (_inTransition) {
            Debug.LogWarning("State Change Failed. State transition already in progress.");
            yield break;
        }

        _inTransition = true;
        _state.enabled = false;
        _state.DoShutdown();

        yield return SceneManager.LoadSceneAsync(levelName);

        if (!string.IsNullOrEmpty(stateName))
        {
            _state = Util.GetComponentsInScene<GameState>().Where(s => s.gameObject.name == stateName).FirstOrDefault();
            if (!_state) {
                Debug.LogError($"GameState script not found: '{stateName}'");
                yield break;
            }
        }
        else
        {
            _state = FindDefaultState();
            if (_state == null) {
                Debug.LogError("No default state was found in this level.");
                yield break;
            }

            args = new object[0];
        }

        yield return null;

        _state.DoPreStart(args);
        _state.DoStartup(args);
        _state.enabled = true;
        _inTransition = false;
    }
}
