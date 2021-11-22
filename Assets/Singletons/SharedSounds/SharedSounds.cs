using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class SharedSounds : MonoBehaviour
{
    public static SharedSounds that
    {
        get; private set;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateInstance()
    {
        if (!that)
        {
            var prefab = Resources.Load<GameObject>("SharedSounds");
            var go = Instantiate(prefab);
            DontDestroyOnLoad(go);
            that = go.GetComponent<SharedSounds>();
        }
    }

    public AudioMixer audioMixer;
    public AudioMixerGroup soundsMixerGroup;

    public Sound2D _button;
    public Sound2D _alarm;
    public Sound2D _error;
    public Sound2D _activate;
    public Sound2D _deactivate;
    public Sound2D _lowBattery;
    public Sound2D _gearPickup;
    public Sound2D _batteryPickup;
    public Sound2D _coinPickup;
    public Sound2D _shortCircuit;
    public Sound2D _explosion;
    public Sound2D _useButton;
    public Sound2D _lasersOn;
    public Sound2D _lasersOff;
    public Sound2D _useBattery;
    public Sound2D _hint;
    public Sound2D _jump;
    public Sound2D _land;
    public Sound2D _gearClank;

    public static Sound2D gearClank { get { return that._gearClank; } }
    public static Sound2D button { get { return that._button; } }
    public static Sound2D alarm { get { return that._alarm; } }
    public static Sound2D error { get { return that._error; } }
    public static Sound2D activate { get { return that._activate; } }
    public static Sound2D deactivate { get { return that._deactivate; } }
    public static Sound2D lowBattery { get { return that._lowBattery; } }
    public static Sound2D gearPickup { get { return that._gearPickup; } }
    public static Sound2D batteryPickup { get { return that._batteryPickup; } }
    public static Sound2D coinPickup { get { return that._coinPickup; } }
    public static Sound2D shortCircuit { get { return that._shortCircuit; } }
    public static Sound2D explosion { get { return that._explosion; } }
    public static Sound2D useButton { get { return that._useButton; } }
    public static Sound2D lasersOn { get { return that._lasersOn; } }
    public static Sound2D lasersOff { get { return that._lasersOff; } }
    public static Sound2D useBattery { get { return that._useBattery; } }
    public static Sound2D hint { get { return that._hint; } }
    public static Sound2D jump { get { return that._jump; } }
    public static Sound2D land { get { return that._land; } }

    public static AudioMixer Mixer {
        get => that.audioMixer;
    }

    public static AudioMixerGroup MixerGroup {
        get => that.soundsMixerGroup;
    }

    public static bool MixerMute
    {
        set {
            Mixer.SetFloat("soundMute", value ? -80 : 0);
        }

        get {
            float volume = 0;
            Mixer.GetFloat("soundMute", out volume);
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
            Mixer.SetFloat("soundVolume", volume);
            Mixer.SetFloat("hintVolume", volume);
        }

        get => _mixerVolume;
    }

    void Awake()
    {
        if (that) {
            Destroy(this.gameObject);
            return;
        }

        that = this;
        DontDestroyOnLoad(gameObject);
    }
}
