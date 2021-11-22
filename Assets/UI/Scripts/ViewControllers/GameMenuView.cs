using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameMenuView : MonoBehaviour
{
    public CanvasGroup group;
    public GameMenuButton menuButton;
    public GameObject playButton;
    public GameObject quitButton;

    GameObject oldSelection;

    public void OnPlayPressed() {
        menuButton.OnPress();
    }

    public void OnQuitPressed()
    {
        menuButton.OnPress();
        MusicMixer.Stop(0.1f);
        GameController.SetLevel(-1);
    }

    private void OnEnable()
    {
        oldSelection = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(playButton);
        StartCoroutine(FadeIn());
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if(oldSelection)
            EventSystem.current.SetSelectedGameObject(oldSelection);
    }

    IEnumerator FadeIn()
    {
        group.alpha = 0;
        var length = 0.25f;
        var startTime = Time.unscaledTime;
        var endTime = startTime + length;

        while(Time.unscaledTime <= endTime)
        {
            float t = (Time.unscaledTime - startTime) / length;
            group.alpha = t;
            yield return null;
        }

        group.alpha = 1.0f;
    }
}
