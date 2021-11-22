using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenuButton : MonoBehaviour
{
    public GameView gameView;
    public GameMenuView gameMenuView;

    public bool IsPaused {
        get; private set;
    }

    float originalTimeScale;
    bool mainJoystickWasActive;
    bool jumpAreaWasActive;
    CameraRenderMode renderMode = CameraRenderMode.Colorful;

    public void OnPress()
    {
        if (!IsPaused)
        {
            IsPaused = true;

            originalTimeScale = Time.timeScale;
            Time.timeScale = 0;
            AudioListener.pause = true;
            renderMode = RenderManager.renderMode;
            RenderManager.renderMode = CameraRenderMode.Grayscale;
            mainJoystickWasActive = gameView.mainJoystick.gameObject.activeSelf;
            jumpAreaWasActive = gameView.jumpArea.gameObject.activeSelf;
            gameView.mainJoystick.SetActive(false);
            gameView.jumpArea.SetActive(false);
            gameMenuView.SetActive(true);
        }
        else
        {
            IsPaused = false;

            Time.timeScale = originalTimeScale;
            AudioListener.pause = false;
            RenderManager.renderMode = renderMode;
            gameView.mainJoystick.SetActive(mainJoystickWasActive);
            gameView.jumpArea.SetActive(jumpAreaWasActive);
            gameMenuView.SetActive(false);
        }
    }

    private void Update()
    {
        if (Gamepad.current.startButton.wasPressedThisFrame)
            OnPress();
    }
}
