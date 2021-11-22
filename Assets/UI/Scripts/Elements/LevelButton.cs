using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LevelButton : MonoBehaviour
{
    public int levelIndex = 0;
    public int gearQuota = 3;

    public GameObject loadingScreen;

    public Button button;
    public CanvasGroup group;

    public RectTransform timeArea;
    public RectTransform coinArea;
    public RectTransform gearArea;

    public Image expertStar;
    public TMP_Text levelNumber;
    public TMP_Text levelTimeText;
    public TMP_Text coinCountText;
    public Image[] uncollectedGears;
    public Image[] collectedGears;
    public Image gearQuotaIcon;
    public TMP_Text gearQuotaText;
    public Image levelLock;

    bool unlocked = false;

    private void OnEnable() {
        UpdateButton();
    }

    void UpdateButton()
    {
        var isUnlocked = Profile.Levels[levelIndex].unlocked;
        var isExpert = Profile.Levels[levelIndex].expertPassed;
        var completionTime = Profile.Levels[levelIndex].completionTime;
        var coinCount = Profile.Levels[levelIndex].coinsCollected;
        var gearCount = Profile.Levels[levelIndex].gearsCollected;
        var batteriesRemaining = Profile.Levels[levelIndex].batteriesRemaining;

        int ct = Mathf.RoundToInt(completionTime);
        int cmins = ct / 60;
        int csecs = ct % 60;

        string strBestTime = ct > 0 ? (cmins + ":" + csecs.ToString("00")) : "--";
        levelTimeText.text = strBestTime;
        coinCountText.text = Util.CalculateTotalCoins(coinCount, gearCount, batteriesRemaining, isExpert).ToString();
        
        if (isUnlocked)
        {
            levelLock.SetActive(false);
            expertStar.SetActive(isExpert);
            timeArea.SetActive(true);

            button.interactable = true;
            group.alpha = 1;

            if(gearQuota == 3)
            {
                coinArea.SetActive(true);
                gearArea.SetActive(true);
                gearQuotaIcon.SetActive(false);
                gearQuotaText.SetActive(false);

                collectedGears[0].SetActive(gearCount > 0);
                collectedGears[1].SetActive(gearCount > 1);
                collectedGears[2].SetActive(gearCount > 2);
                uncollectedGears[0].SetActive(gearCount <= 0);
                uncollectedGears[1].SetActive(gearCount <= 1);
                uncollectedGears[2].SetActive(gearCount <= 2);
            }
            else
            {
                coinArea.SetActive(false);
                gearArea.SetActive(false);
                gearQuotaIcon.SetActive(true);
                gearQuotaText.SetActive(true);
                gearQuotaText.text = gearCount + "/" + gearQuota;
            }
        }
        else
        {
            levelLock.SetActive(true);
            expertStar.SetActive(false);
            timeArea.SetActive(false);
            coinArea.SetActive(false);
            gearArea.SetActive(false);
            group.alpha = 0.7f;
            button.interactable = false;

            gearQuotaIcon.SetActive(false);
            gearQuotaText.SetActive(false);
        }

        unlocked = isUnlocked;

        StartCoroutine(UpdateLayout());
    }

    IEnumerator UpdateLayout()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(timeArea);
        LayoutRebuilder.ForceRebuildLayoutImmediate(coinArea);
        LayoutRebuilder.ForceRebuildLayoutImmediate(gearArea);
    }

    public void OnPress()
    {
        SharedSounds.button.Play();

        if(unlocked) {
            StartCoroutine(LoadLevel());
            GameController.SetLevel(levelIndex);
        }
    }

    IEnumerator LoadLevel()
    {
        loadingScreen.SetActive(true);
        
        yield return new WaitForSeconds(0.1f);

        GameController.SetLevel(levelIndex);
    }
}
