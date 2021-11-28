using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.InputSystem;

public class ScreenShotter : MonoBehaviour
{
    public int scale = 1;
    int screenshot = 0;

    void Start() {
        DontDestroyOnLoad(gameObject);
    }

    bool screenshotMode = false;

    private void OnLevelWasLoaded(int level)
    {
        screenshotMode = false;
    }

    void Update()
    {
        
        if (Keyboard.current.WasReleasedThisFrame(Key.Enter))
        {
            if(!screenshotMode)
            {
                // Stop game so multiple screenshots can be taken.
                // Press "End" to resume game.
                Time.timeScale = 0;
                screenshotMode = true;
            }

            var levelName = SceneManager.GetActiveScene().name;
            var levelTime = TimeSpan.FromSeconds(Time.timeSinceLevelLoad).ToString("mm\\.ss\\.fff");
            var resolution = $"{Screen.width}x{Screen.height}";

            if(!Directory.Exists("Screenshots"))
                Directory.CreateDirectory("Screenshots");

            var filename = $"Screenshots/{levelName}_{levelTime}_{resolution}.png";
            ScreenCapture.CaptureScreenshot(filename);

            Debug.Log("image saved: " + filename);
        }
        
        if (Keyboard.current.WasReleasedThisFrame(Key.End))
        {
            if(screenshotMode)
            {
                Time.timeScale = 1;
                screenshotMode  = false;
            }
        }
    }
}
