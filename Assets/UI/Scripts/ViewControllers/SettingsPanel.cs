using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsPanel : ViewController
{
    public ToggleButton musicToggle;
    public ToggleButton soundToggle;
    public SettingsJoystick joystick;

    bool musicEnabled;
    bool soundEffectsEnabled;

    private void OnEnable()
    {
        musicEnabled = Profile.MusicEnabled;
        musicToggle.SetState(musicEnabled, false);
        soundEffectsEnabled = Profile.SoundEffectsEnabled;
        soundToggle.SetState(soundEffectsEnabled, false);
        joystick.Limit = Profile.JoystickLimit;
    }

    private void OnDisable()
    {
        Profile.MusicEnabled = musicEnabled;
        Profile.SoundEffectsEnabled = soundEffectsEnabled;
        Profile.JoystickLimit = joystick.Limit;
        Profile.Save();
    }

    public void OnMusicToggled(ToggleButton button)
    {
        musicEnabled = button.State;
        MusicMixer.MixerMute = !musicEnabled;
    }

    public void OnSoundEffectsToggled(ToggleButton button)
    {
        soundEffectsEnabled = button.State;
        SharedSounds.MixerMute = !soundEffectsEnabled;

        if (soundEffectsEnabled)
            SharedSounds.button.Play();
    }
}
