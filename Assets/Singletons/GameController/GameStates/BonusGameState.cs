using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BonusGameState : GameState
{
    private LevelStart levelStart;
    private LevelEnd levelEnd;

    private int _levelIndex;
    private float _startTime = -1;
    private bool _isRunning = false;

    public GameView gameView;
    public Material playerRefractiveMaterial;
    public Material playerNormalMaterial;
    public AudioClip warpInSound;
    public AudioClip blipSound;
    public AudioClip materializeSound;
    public Camera camera_showPlayerAppear;

    public int bonusGearQuota = 300;
    //public int bonusTimeLimit = 5 * 60;
    
    public override bool isRunning {
        get { return _isRunning; }
    }

    public override void DoPreStart(object[] args)
    {
        levelStart = GameObject.Find("LevelStart").GetComponent<LevelStart>();
        levelEnd = GameObject.Find("LevelEnd").GetComponent<LevelEnd>();
        InitUI();
    }

    public override void Startup(object[] args)
    {
        StartCoroutine(_startGame());
    }

    public override void Shutdown()
    {
        LevelResult result;

        if(gameView.bonusGearCounter.gearCount == bonusGearQuota)
            result = LevelResult.Win;
        else if(timeRemaining == 0)
            result = LevelResult.Loss;
        else
            result = LevelResult.Quit;

        FinishGame(result, gameView.bonusGearCounter.gearCount, 0);
    }

    // ***********

    public override void OnPlayerDamaged(int amount)
    {
        // player doesn't take damage in bonus level
    }

    public override void UseBatteries(int count)
    {
        // cannot use batteries in bonus level
    }

    public override void OnBatteriesCollected(int count, Vector3? fromWorldPosition)
    {
        // cannot collect batteries in bonus level
    }

    public override void OnCoinsCollected(int count, Vector3? fromWorldPosition)
    {
        // cannot collect coins in bonus level
    }

    public override void OnGearsCollected(Vector3 fromWorldPosition)
    {
        gameView.bonusGearCounter.AddGear(fromWorldPosition);
        SharedSounds.gearPickup.Play();
    }

    public override void OnUseTriggerActivated(UseTrigger trigger)
    {
        gameView.usePanel.target = trigger.target;
    }

    public override void OnUseTriggerDeactivated(UseTrigger trigger)
    {
        gameView.usePanel.target = null;
    }

    public void OnBackPressed()
    {
        MessageOverlay.Show(Strings.QUIT_MESSAGE, MessageBoxButtons.OKCancel, mb => {
            if (mb.Result == MessageBoxResult.OK)
                GameController.SetLevel(-1);
        });

        MessageOverlay.Instance.Animate();
    }

    // ***********
    
    int timeRemaining
    {
        get
        {
            if(_startTime < 0)
                return Difficulty.bonusTimeLimit;
            
            return Mathf.Max(Difficulty.bonusTimeLimit - Mathf.RoundToInt(Time.time - _startTime), 0);
        }
    }

    void Update()
    {
        if(!_isRunning)
            return;
        
        int remaining = this.timeRemaining;

        if(Keyboard.current.kKey.wasReleasedThisFrame)
            remaining = 0;

        gameView.bonusTimer.seconds = remaining;

        if(gameView.bonusGearCounter.gearCount == bonusGearQuota)
        {
            StartCoroutine(_endSuccess());
        }
        else if(remaining == 0)
        {
            StartCoroutine(_endFailure());
        }
    }

    void InitUI()
    {
        gameView.bonusGearCounter.gearQuota = bonusGearQuota;
        gameView.bonusTimer.seconds = Difficulty.bonusTimeLimit;
    }

    IEnumerator _startGame()
    {
        Player.useBattery = false;
        Player.battery = 100;
        Player.that.anim.Play("Default");
        Player.that.shadowCaster.SetActive(false);
        Player.that.slipstreamBadge.Online = true;

        yield return new WaitForSeconds(1.0f);

        CameraController.that.ShowView(camera_showPlayerAppear, 0);
        Player.that.SetActive(false);

        RenderManager.renderMode = CameraRenderMode.Colorful;
        RenderManager.FadeToColor(Color.clear, 1.0f);

        yield return new WaitForSeconds(2.0f);

        float maxPinch = 0.8f;

        // teleport player into game
        Player.that.SetActive(true);
        Player.that.slipstreamBadge.UseStationReflection();
        playerRefractiveMaterial.SetFloat("_Flatten", 1.0f);
        playerRefractiveMaterial.SetFloat("_EmissionAmount", 0.0f);
        playerRefractiveMaterial.SetFloat("_Cutoff", 1.0f);

        RenderManager.renderEffect = RenderEffect.Pinch;

        Util.PlayClip(warpInSound);
        yield return StartCoroutine(Util.Blend(0.3f, t => {
            playerRefractiveMaterial.SetFloat("_Flatten", Curve.OutQuad(t));
            playerRefractiveMaterial.SetFloat("_EmissionAmount", Curve.InLinear(t));
            RenderManager.pinchAmplitude = Mathf.Lerp(0, -maxPinch, Curve.InCube(t));
        }));

        var src = Util.PlayClip(materializeSound, 0.5f);
        yield return StartCoroutine(Util.Blend(1.5f, t => {
            playerRefractiveMaterial.SetFloat("_Cutoff", Curve.OutLinear(t));
            var amp = Curve.Sine(0.25f + t * 6.0f) * Curve.OutCube(t) * -maxPinch;
            RenderManager.pinchAmplitude = amp;
            if(src) src.volume = Curve.OutQuad(t) * 0.5f;
        }));
        src = null;

        RenderManager.renderEffect = RenderEffect.None;

        Player.that.SetMaterial(playerNormalMaterial);
        Player.that.shadowCaster.SetActive(true);
        yield return new WaitForSeconds(1.0f);

        Util.PlayClip(blipSound, 0.5f);
        Player.that.slipstreamBadge.Online = false;

        yield return new WaitForSeconds(1.0f);

        Player.that.anim.Play("Default");
        Player.that.anim.Sample();
        yield return null;
        Player.that.anim.CrossFade("Idle");

        CameraController.that.FollowPlayer();
        yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(0.25f);

        gameView.bonusGearCounter.gearQuota = bonusGearQuota;
        gameView.bonusGearCounter.SetActive(true);
        gameView.bonusTimer.seconds = Difficulty.bonusTimeLimit;
        gameView.bonusTimer.SetActive(true);
        gameView.mainJoystick.SetActive(true);
        gameView.jumpArea.SetActive(true);
        Player.that.enabled = true;

        _levelIndex = Util.levelIndex;
        _startTime = Time.time;
        _isRunning = true;

        StartGame();
    }
    
    IEnumerator _endSuccess()
    {
        _isRunning = false;
        Player.that.enabled = false;
        Player.that.anim.Play("Idle");
        levelEnd.bigDoors.isOpen = false;
        gameView.bonusGearCounter.SetActive(false);
        gameView.bonusTimer.SetActive(false);
        gameView.mainJoystick.SetActive(false);
        gameView.jumpArea.SetActive(false);
        
        float completionTime = Mathf.Max(Time.time - _startTime, 0);
        int gearCount = gameView.bonusGearCounter.gearCount;
        int coinCount = 0;
        int battieriesRemaining = 0;
        bool expertPassed = Difficulty.level == DifficultyLevel.Hard;

        // award 1000 coins for collecting all gears in time
        coinCount += 1000;

        Commit(_levelIndex, completionTime, gearCount, coinCount, battieriesRemaining, expertPassed);

        MusicMixer.Stop(1.0f);
        yield return new WaitForSeconds(1.0f);

        gameView.resultView.Show(_levelIndex, true, gearCount, bonusGearQuota, completionTime, expertPassed);

        yield break;
    }

    IEnumerator _endFailure()
    {
        _isRunning = false;
        Player.that.enabled = false;
        Player.that.anim.Play("Idle");
        gameView.bonusGearCounter.SetActive(false);
        gameView.bonusTimer.SetActive(false);
        gameView.mainJoystick.SetActive(false);
        gameView.jumpArea.SetActive(false);

        float completionTime = Difficulty.bonusTimeLimit;
        int batteriesRemaining = 0;
        int coinCount = 0;
        int gearCount = gameView.bonusGearCounter.gearCount;
        bool expertPassed = false;

        Commit(_levelIndex, completionTime, gearCount, coinCount, batteriesRemaining, expertPassed);

        RenderManager.FadeToColor(new Color(0, 0, 0, 0.7f), 1.0f);
        MusicMixer.Stop(1.0f);
        yield return new WaitForSeconds(1.0f);

        gameView.resultView.Show(_levelIndex, false, gearCount, bonusGearQuota, completionTime, expertPassed);
        
        yield break;
    }
}
