using UnityEngine;
using System.Collections;

public class Defaults
{
    public static readonly Vector2 roundButtonSize = new Vector2(192, 192);
    public const float screenBorder = 22;
    public const float popupMargin = 30;
    public const float lightIntensity = 1.0f;
    public static readonly Color ambientColor = new Color32(214, 181, 202, 0); //D6B5CA
    public const float lightOffIntensity = lightIntensity * 0.35f;
    public static readonly Color ambientOffColor = ambientColor * 0.45f;
    public const int coinsPerAd = 500;
    public static readonly Vector2 joystickSize = new Vector2(192, 192);
    public static float minJoystickOffset = 0.1f;
    public static float maxJoystickOffset = 3.0f;

    public const int CoinsPerBattery =  100;
    public const int CoinsPerGear = 100;
    public const int CoinsForExpertMode = 500;
    public const int CoinsForBonusCompletion = 1000;
    public const int CoinExpertMultiplier = 2;
}
