using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLight : MonoBehaviour
{
    private static Light _light = null;
    public static Light Light {
        get
        {
            if (!_light)
                _light = FindObjectOfType<MainLight>(true).GetComponent<Light>();

            return _light;
        }
    }

    public static void ApplyNormalLightsSettings()
    {
        Light.color = new Color32(196, 140, 140, 0);
        Light.intensity = Defaults.lightIntensity;
        RenderSettings.ambientLight = Defaults.ambientColor;
    }

    public static void ApplyOffLightsSettings()
    {
        Light.color = new Color32(196, 140, 140, 0);
        Light.intensity = Defaults.lightOffIntensity;
        RenderSettings.ambientLight = Defaults.ambientOffColor;
    }

    public static void ApplyDimmedLightSettings()
    {
        Light.color = new Color32(196, 140, 140, 0);
        Light.intensity = Defaults.lightIntensity * 0.7f;
        RenderSettings.ambientLight = Defaults.ambientColor * 0.7f;
    }

    public static void ApplyDystopiaLightSettings()
    {
        //Light.color = new Color32(67, 25, 0, 0);
        Light.color = new Color32(132, 85, 58, 0);
        Light.intensity = Defaults.lightIntensity;
        //RenderSettings.ambientLight = new Color32(108, 55, 33, 0);
        RenderSettings.ambientLight = new Color32(94, 67, 56, 0);
    }
}
