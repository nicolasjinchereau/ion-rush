using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum MessageBoxResult
{
    None,
    OK,
    Yes,
    No,
    Cancel,
    Abort,
    Ignore,
    Retry,
}

public enum MessageBoxButtons
{
    OK,
    OKCancel,
    YesNo,
    YesNoCancel,
    RetryCancel,
    AbortRetryIgnore,
}

public delegate void MessageOverlayCallback(MessageOverlay messageBox);

public class MessageOverlay : MonoBehaviour
{
    static MessageOverlay _instance = null;

    public static MessageOverlay Instance
    {
        get
        {
            if(_instance == null)
                _instance = Util.GetComponentInScene<MessageOverlay>();
            
            return _instance;
        }
    }

    public RectTransform background;

    public RectTransform captionContainer;
    public TMP_Text captionText;

    public RectTransform messageContainer;
    public TMP_Text messageText;

    public Button button1;
    public Button button2;
    public Button button3;

    public TMP_Text buttonText1;
    public TMP_Text buttonText2;
    public TMP_Text buttonText3;

    MessageBoxResult buttonResult1;
    MessageBoxResult buttonResult2;
    MessageBoxResult buttonResult3;

    public MessageBoxResult Result {
        get; private set;
    }

    public MessageOverlayCallback Callback {
        get; set;
    }

    //private void Awake() {
    //    Instance = this;
    //}

    public static void Show(string message, MessageOverlayCallback callback) {
        Instance.DoShow(message, "", MessageBoxButtons.OK, null, callback);
    }

    public static void Show(string message, MessageBoxButtons buttons, MessageOverlayCallback callback) {
        Instance.DoShow(message, "", buttons, null, callback);
    }

    public static void Show(string message, string caption, MessageOverlayCallback callback) {
        Instance.DoShow(message, caption, MessageBoxButtons.OK, null, callback);
    }

    public static void Show(string message, string caption, MessageBoxButtons buttons, MessageOverlayCallback callback = null) {
        Instance.DoShow(message, caption, buttons, null, callback);
    }

    public static void Show(string message, string caption, MessageBoxButtons buttons, Sprite icon, MessageOverlayCallback callback) {
        Instance.DoShow(message, caption, buttons, icon, callback);
    }

    public void DoShow(string message, string caption, MessageBoxButtons buttons, Sprite icon, MessageOverlayCallback callback)
    {
        gameObject.SetActive(true);

        messageContainer.gameObject.SetActive(!string.IsNullOrEmpty(message));
        messageText.text = message;

        captionContainer.gameObject.SetActive(!string.IsNullOrEmpty(caption));
        captionText.text = caption;

        Result = MessageBoxResult.None;
        Callback = callback;

        switch(buttons)
        {
            case MessageBoxButtons.OK:
                {
                    button1.gameObject.SetActive(true);
                    button2.gameObject.SetActive(false);
                    button3.gameObject.SetActive(false);

                    buttonText1.text = Localizer.Get("MB_OK");
                    buttonResult1 = MessageBoxResult.OK;
                    break;
                }
            case MessageBoxButtons.OKCancel:
                {
                    button1.gameObject.SetActive(true);
                    button2.gameObject.SetActive(true);
                    button3.gameObject.SetActive(false);

                    buttonText1.text = Localizer.Get("MB_CANCEL");
                    buttonText2.text = Localizer.Get("MB_OK");
                    buttonResult1 = MessageBoxResult.Cancel;
                    buttonResult2 = MessageBoxResult.OK;
                    break;
                }
            case MessageBoxButtons.YesNo:
                {
                    button1.gameObject.SetActive(true);
                    button2.gameObject.SetActive(true);
                    button3.gameObject.SetActive(false);

                    buttonText1.text = Localizer.Get("MB_NO");
                    buttonText2.text = Localizer.Get("MB_YES");
                    buttonResult1 = MessageBoxResult.No;
                    buttonResult2 = MessageBoxResult.Yes;
                    break;
                }
            case MessageBoxButtons.YesNoCancel:
                {
                    button1.gameObject.SetActive(true);
                    button2.gameObject.SetActive(true);
                    button3.gameObject.SetActive(true);

                    buttonText1.text = Localizer.Get("MB_CANCEL");
                    buttonText2.text = Localizer.Get("MB_NO");
                    buttonText3.text = Localizer.Get("MB_YES");
                    buttonResult1 = MessageBoxResult.Cancel;
                    buttonResult2 = MessageBoxResult.No;
                    buttonResult3 = MessageBoxResult.Yes;
                    break;
                }
            case MessageBoxButtons.RetryCancel:
                {
                    button1.gameObject.SetActive(true);
                    button2.gameObject.SetActive(true);
                    button3.gameObject.SetActive(false);

                    buttonText1.text = Localizer.Get("MB_CANCEL");
                    buttonText2.text = Localizer.Get("MB_RETRY");
                    buttonResult1 = MessageBoxResult.Cancel;
                    buttonResult2 = MessageBoxResult.Retry;
                    break;
                }
            case MessageBoxButtons.AbortRetryIgnore:
                {
                    button1.gameObject.SetActive(true);
                    button2.gameObject.SetActive(true);
                    button3.gameObject.SetActive(true);
                    buttonText1.text = Localizer.Get("MB_IGNORE");
                    buttonText2.text = Localizer.Get("MB_ABORT");
                    buttonText3.text = Localizer.Get("MB_RETRY");
                    buttonResult1 = MessageBoxResult.Ignore;
                    buttonResult2 = MessageBoxResult.Abort;
                    buttonResult3 = MessageBoxResult.Retry;
                    break;
                }
        }
    }

    void Finish(MessageBoxResult result)
    {
        if(Result == MessageBoxResult.None)
        {
            SharedSounds.button.Play();
            gameObject.SetActive(false);
            Result = result;
            Callback?.Invoke(this);
        }
    }

    public void OnButton1Pressed() {
        Finish(buttonResult1);
    }

    public void OnButton2Pressed() {
        Finish(buttonResult2);
    }

    public void OnButton3Pressed() {
        Finish(buttonResult3);
    }

    public void Animate() {
        StartCoroutine(_animate());
    }

    IEnumerator _animate()
    {
        var rt = transform as RectTransform;

        Vector2 startPos = new Vector2(0, -rt.rect.size.y);
        Vector2 goalPos = Vector2.zero;
        background.anchoredPosition = startPos;

        yield return new WaitForSeconds(0.1f);

        SharedSounds.hint.Play();

        yield return Util.Blend(1.1f, t => {
            t = Curve.InElastic(t);
            background.anchoredPosition = startPos + (goalPos - startPos) * t;
        });
    }
}
