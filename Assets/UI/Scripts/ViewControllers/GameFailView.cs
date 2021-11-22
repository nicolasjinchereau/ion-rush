using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class GameFailView : MonoBehaviour
{
    public TMP_Text levelText;
    public TMP_Text hintText;
    public AudioClip failSound;

    public GameObject quitButton;
    public GameObject retryButton;

    GameObject oldSelection;

    private void OnEnable()
    {
        oldSelection = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(retryButton);
    }

    private void OnDisable()
    {
        if(oldSelection)
            EventSystem.current.SetSelectedGameObject(oldSelection);
    }

    public void OnQuitPressed()
    {
        MusicMixer.Stop(0.3f);
        GameController.SetLevel(-1);
    }

    public void OnRetryPressed()
    {
        MusicMixer.Stop(0.3f);
        GameController.SetLevel(Util.levelIndex);
    }

    public void Show(int level, string failureHint)
    {
        Util.PlayClip(failSound, 0.8f);
        levelText.text = $"LEVEL {level + 1}";
        gameObject.SetActive(true);
        hintText.text = failureHint;
    }
}
