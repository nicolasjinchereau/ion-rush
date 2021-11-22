using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExpertButton : MonoBehaviour
{
    public RectTransform rectTransform;
    public Image background;
    public Image slider;
    public Image warningIcon;

    float minX = -52;
    float maxX = 52;
    
    bool isOn = false;
    bool isAnimating = false;
    
    public void OnPress()
    {
        isOn = !isOn;
        isAnimating = true;

        if (isOn) {
            Difficulty.level = DifficultyLevel.Hard;
            Profile.DifficultyLevel = DifficultyLevel.Hard;
        }
        else {
            Difficulty.level = DifficultyLevel.Easy;
            Profile.DifficultyLevel = DifficultyLevel.Easy;
        }

        warningIcon.gameObject.SetActive(false);
        
        SharedSounds.button.Play();
    }

    private void OnEnable()
    {
        if(Profile.DifficultyLevel == DifficultyLevel.Hard) {
            warningIcon.gameObject.SetActive(true);
            slider.rectTransform.anchoredPosition = new Vector2(maxX, slider.rectTransform.anchoredPosition.y);
        }
        else {
            warningIcon.gameObject.SetActive(false);
            slider.rectTransform.anchoredPosition = new Vector2(minX, slider.rectTransform.anchoredPosition.y);
        }
    }

    const float SliderSpeed = 3.0f;

    void Update()
    {
        if(isAnimating)
        {
            var sliderPos = slider.rectTransform.anchoredPosition;

            if(isOn)
            {
                var x = sliderPos.x + Time.deltaTime * (maxX - minX) * SliderSpeed;
                sliderPos.x = Mathf.Min(x, maxX);

                if(x >= maxX)
                    isAnimating = false;
            }
            else
            {
                var x = sliderPos.x - Time.deltaTime * (maxX - minX) * SliderSpeed;
                sliderPos.x = Mathf.Max(x, minX);

                if(x <= minX)
                    isAnimating = false;
            }

            slider.rectTransform.anchoredPosition = sliderPos;

            if(!isAnimating)
            {
                if (isOn) {
                    warningIcon.gameObject.SetActive(true);
                    SharedSounds.alarm.Play();
                }
            }
        }
    }
}
