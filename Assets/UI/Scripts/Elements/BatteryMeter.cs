using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class BatteryMeter : MonoBehaviour
{
    public Image[] bars;
    public Image reviveArrow;
    public Image buttonSideTab;
    public Button batteryButton;

    public float level = 0.0f;
    public bool showBatteryButton = false;

    Color barColor = new Color32(175, 248, 255, 210);
    int flashCount = 0;
    float flashColor = 0;
    float nextFlash = 0;
    float flashDelay = 0.28f;

    bool reviveArrowEnabled = false;
    float batButtonPos = 0.0f;
    Coroutine _reviveRoutine = null;

    public void Update()
    {
        if(showBatteryButton)
            batButtonPos = Mathf.Min(batButtonPos + Time.deltaTime * 5.4f, 1.0f);
        else
            batButtonPos = Mathf.Max(batButtonPos - Time.deltaTime * 5.4f, 0.0f);

        var batTabPosX = Mathf.Lerp(138, 248, batButtonPos);
        buttonSideTab.rectTransform.anchoredPosition = new Vector2(batTabPosX, 0);
        batteryButton.interactable = batButtonPos > 0.9f;

        // update battery meter bars
        this.level = Player.exists ? Mathf.Clamp01(Player.battery / 100.0f) : 0;

        if(flashCount > 0 && Time.time > nextFlash)
        {
            --flashCount;
            flashColor = 1.0f;
            nextFlash = Time.time + flashDelay;
        }

        if(flashColor > 0.001f)
            flashColor = Mathf.Max(flashColor - Time.deltaTime * 3.0f, 0.0f);
        else
            flashColor = 0.0f;

        var currentBarColor = Color.Lerp(barColor, Color.red, flashColor);

        var count = bars.Length;
        var barWidth = 1.0f / count;

        for(int i = 0; i < count; ++i)
        {
            float alpha = Mathf.Clamp01((level - i * barWidth) / barWidth);

            var col = currentBarColor;
            col.a *= alpha;
            bars[i].color = col;
        }
        
        if (batteryButton.interactable &&
            (Keyboard.current.WasPressedThisFrame(Key.B) || Gamepad.current.WasPressedThisFrame(GamepadButton.B)))
        {
            OnBatteryPressed();
        }
    }

    public void OnBatteryPressed()
    {
        SharedSounds.useBattery.Play();
        GameController.state.UseBatteries(1);
    }

    public void DoBarFlash(int flashes = 3) {
        flashCount = flashes;
    }

    public void StartReviveArrow()
    {
        if(!reviveArrowEnabled)
        {
            reviveArrowEnabled = true;
            _reviveRoutine = StartCoroutine(_doReviveArrow());
        }
    }

    public void StopReviveArrow()
    {
        reviveArrowEnabled = false;

        if(_reviveRoutine == null)
            reviveArrow.SetActive(false);
    }

    IEnumerator _doReviveArrow()
    {
        yield return null;

        var canvas = GetComponentInParent<Canvas>();
        var canvasTransform = canvas.gameObject.GetComponent<RectTransform>();
        var screenSize = canvasTransform.sizeDelta;

        float right = screenSize.x * 0.5f + reviveArrow.rectTransform.sizeDelta.x * 0.5f;

        var tabRT = buttonSideTab.rectTransform;
        float left = tabRT.anchoredPosition.x + tabRT.sizeDelta.x * 0.5f +
                     reviveArrow.rectTransform.sizeDelta.x * 0.5f + 10;

        reviveArrow.rectTransform.anchoredPosition = new Vector2(right, 0);
        reviveArrow.SetActive(true);

        yield return new WaitForSeconds(1.0f);

        yield return Util.Blend(1.0f, t =>
        {
            float x = Mathf.Lerp(right, left, Curve.BounceIn(t));
            reviveArrow.rectTransform.anchoredPosition = new Vector2(x, 0);
        });

        yield return new WaitForSeconds(0.5f);

        right = left + reviveArrow.rectTransform.anchoredPosition.x * 0.3f;

        yield return Util.Blend(0.5f, t =>
        {
            float x = Mathf.Lerp(left, right, Curve.OutQuadInv(t));
            reviveArrow.rectTransform.anchoredPosition = new Vector2(x, 0);
        });

        yield return Util.Blend(1.0f, t =>
        {
            float x = Mathf.Lerp(right, left, Curve.BounceIn(t));
            reviveArrow.rectTransform.anchoredPosition = new Vector2(x, 0);
        });

        yield return new WaitForSeconds(0.5f);

        if(!reviveArrowEnabled)
            reviveArrow.SetActive(false);
        
        _reviveRoutine = null;
    }
}
