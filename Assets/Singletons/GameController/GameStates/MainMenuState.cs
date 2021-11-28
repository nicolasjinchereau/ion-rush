using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MainMenuState : GameState
{
    public EventSystem eventSystem;
    public HomePanel homePanel;
    public LevelPanel levelPanel;
    public StorePanel storePanel;
    public HelpPanel helpPanel;
    public SettingsPanel settingsPanel;

    public GameObject homePanelSelection;
    public GameObject levelPanelSelection;
    public GameObject settingsPanelSelection;
    public GameObject helpPanelSelection;
    public GameObject storePanelSelection;

    ViewController currentPanel;
    bool inTransition = false;

    public override void Startup(object[] args)
    {
        Strings.language = Profile.Language;
        Difficulty.level = Profile.DifficultyLevel;
        StartCoroutine(_doStartUp());
    }
    
    IEnumerator _doStartUp()
    {
        yield return new WaitForSeconds(0.5f);    
        RenderManager.renderMode = CameraRenderMode.Normal;
        RenderManager.renderEffect = RenderEffect.None;
        RenderManager.FadeToColor(Color.clear, 1.0f);
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void SwitchToLevels() {
        eventSystem.SetSelectedGameObject(levelPanelSelection);
        StartCoroutine(SwitchToPanelCoop(levelPanel, new Vector3(0, homePanel.rectTransform.rect.height)));
    }

    public void SwitchFromLevels() {
        eventSystem.SetSelectedGameObject(homePanelSelection);
        StartCoroutine(SwitchFromPanelCoop(levelPanel, new Vector3(0, homePanel.rectTransform.rect.height)));
    }

    public void SwitchToSettings() {
        eventSystem.SetSelectedGameObject(settingsPanelSelection);
        StartCoroutine(SwitchToPanelCoop(settingsPanel, new Vector3(-homePanel.rectTransform.rect.width, 0)));
    }

    public void SwitchFromSettings() {
        eventSystem.SetSelectedGameObject(homePanelSelection);
        StartCoroutine(SwitchFromPanelCoop(settingsPanel, new Vector3(-homePanel.rectTransform.rect.width, 0)));
    }

    public void SwitchToHelp() {
        eventSystem.SetSelectedGameObject(helpPanelSelection);
        StartCoroutine(SwitchToPanelCoop(helpPanel, new Vector3(0, -homePanel.rectTransform.rect.height)));
    }

    public void SwitchFromHelp() {
        eventSystem.SetSelectedGameObject(homePanelSelection);
        StartCoroutine(SwitchFromPanelCoop(helpPanel, new Vector3(0, -homePanel.rectTransform.rect.height)));
    }

    public void SwitchToStore() {
        eventSystem.SetSelectedGameObject(storePanelSelection);
        StartCoroutine(SwitchToPanelCoop(storePanel, new Vector3(homePanel.rectTransform.rect.width, 0)));
    }

    public void SwitchFromStore() {
        eventSystem.SetSelectedGameObject(homePanelSelection);
        StartCoroutine(SwitchFromPanelCoop(storePanel, new Vector3(homePanel.rectTransform.rect.width, 0)));
    }

    IEnumerator SwitchToPanelCoop(ViewController panel, Vector3 panelPos, System.Action after = null)
    {
        if(inTransition)
            yield break;

        inTransition = true;
        homePanel.canvasGroup.interactable = false;
        panel.rectTransform.anchoredPosition = panelPos;
        panel.gameObject.SetActive(true);

        yield return Util.Blend(0.5f, t => {
            t = Curve.SmoothStepInSteep(t);
            panel.rectTransform.anchoredPosition = (1.0f - t) * panelPos;
            homePanel.rectTransform.anchoredPosition = t * -panelPos;
        });

        homePanel.gameObject.SetActive(false);
        panel.canvasGroup.interactable = true;

        currentPanel = panel;

        after?.Invoke();

        inTransition = false;
    }

    IEnumerator SwitchFromPanelCoop(ViewController panel, Vector3 panelPos, System.Action before = null)
    {
        if(inTransition)
            yield break;

        inTransition = true;

        before?.Invoke();

        panel.canvasGroup.interactable = false;
        homePanel.rectTransform.anchoredPosition = -panelPos;
        homePanel.gameObject.SetActive(true);

        yield return Util.Blend(0.5f, t => {
            t = Curve.SmoothStepInSteep(t);
            panel.rectTransform.anchoredPosition = t * panelPos;
            homePanel.rectTransform.anchoredPosition = (1.0f - t) * -panelPos;
        });

        panel.gameObject.SetActive(false);
        homePanel.canvasGroup.interactable = true;

        currentPanel = homePanel;

        inTransition = false;
    }
}
