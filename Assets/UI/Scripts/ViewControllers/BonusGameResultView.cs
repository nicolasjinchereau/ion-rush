using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BonusGameResultView : MonoBehaviour
{
    public RectTransform rectTransform;
    public RectTransform titleAreaRect;
    public RectTransform gearCountAreaRect;
    public RectTransform timeAreaRect;

    public TMP_Text levelText;
    public TMP_Text passText;
    public TMP_Text failText;

    public TMP_Text gearCountText;
    public TMP_Text timeText;

    public GameObject expertStar;

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

    public void Show(int level, bool passed, int gearCount, int gearQuota, float completionTime, bool expertPassed)
    {
        levelText.text = $"LEVEL {level + 1}";
        passText.SetActive(passed);
        failText.SetActive(!passed);
        gearCountText.text = $"{gearCount}/{gearQuota}";
        timeText.text = $"{(int)completionTime / 60}:{(int)completionTime % 60:00}";
        expertStar.SetActive(expertPassed);

        gameObject.SetActive(true);

        StartCoroutine(DoLayout());
    }

    IEnumerator DoLayout()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(titleAreaRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(gearCountAreaRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(timeAreaRect);
    }
}
