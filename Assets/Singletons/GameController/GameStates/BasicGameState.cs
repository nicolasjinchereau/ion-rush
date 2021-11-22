using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BasicGameState : GameState
{
    [Serializable]
    public class GameStartEvent : UnityEvent
    {
    }

    public LevelStart levelStart;
    public LevelEnd levelEnd;
    public GameView gameView;
    public GameObject playerPrefab;
    
    public GameStartEvent onGameStart;

    public bool isFirstLevel = false;

    int _levelIndex;
    float _startTime;
    bool _isRunning = false;
    bool _playerDead = false;
    
    public override bool isRunning {
        get { return _isRunning; }
    }

    public override void Startup(object[] args)
    {
        levelStart = FindObjectOfType<LevelStart>(true);
        levelEnd = FindObjectOfType<LevelEnd>(true);
        InitializeUI();
        StartCoroutine(_startGame());
    }

    public override void Shutdown()
    {
        LevelResult result;

        if(levelEnd.playerDidExit)
            result = LevelResult.Win;
        else if(_playerDead)
            result = LevelResult.Loss;
        else
            result = LevelResult.Quit;
        
        FinishGame(result, gameView.gearCounter.gearCount, gameView.coinCounter.coinCount);
    }

    public override void OnPlayerDamaged(int amount)
    {
        gameView.batteryMeter.DoBarFlash(amount);
        gameView.shortCircuit.DoShortCircuit();
    }

    public override void UseBatteries(int count)
    {
        if(gameView.batteryCounter.batteryCount >= count)
        {
            gameView.batteryCounter.batteryCount -= count;
            Player.that.ChargeBattery(count * Difficulty.batteryPickupCharge);

            if(gameView.batteryCounter.batteryCount == 0)
                gameView.batteryMeter.showBatteryButton = false;
        }
        else
        {
            SharedSounds.lowBattery.Play();
        }
    }

    public override void OnBatteriesCollected(int count, Vector3? fromWorldPosition)
    {
        if(fromWorldPosition.HasValue)
            gameView.batteryCounter.AddBatteries(fromWorldPosition.Value, count);
        else
            gameView.batteryCounter.batteryCount += count;

        gameView.batteryMeter.showBatteryButton = true;
        SharedSounds.batteryPickup.Play();
    }

    public override void OnCoinsCollected(int count, Vector3? fromWorldPosition)
    {
        if(fromWorldPosition.HasValue)
            gameView.coinCounter.AddCoins(fromWorldPosition.Value, count);
        else
            gameView.coinCounter.coinCount += count;

        SharedSounds.coinPickup.Play();
    }

    public override void OnGearsCollected(Vector3 fromWorldPosition)
    {
        gameView.gearCounter.AddGear(fromWorldPosition);
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

    void Update()
    {
        if(!_isRunning || !Player.exists)
            return;

#if UNITY_EDITOR
        if(Keyboard.current.kKey.wasReleasedThisFrame)
            Player.battery = 0;
        else if(Keyboard.current.yKey.wasReleasedThisFrame) {
            gameView.gearCounter.gearCount = 2;
            gameView.coinCounter.coinCount = 350;
            levelEnd.playerDidExit = true;
        }
#endif
        
        if(levelEnd.playerDidExit)
        {
            StartCoroutine(_endSuccess());
        }
        else if(Player.isBatteryDead)
        {
            StartCoroutine(_killPlayer());
        }
    }

    void InitializeUI()
    {
        gameView.mainJoystick.SetActive(false);
        gameView.jumpArea.SetActive(false);

        gameView.gearCounter.gearQuota = 3;
        gameView.gearCounter.SetActive(false);
        gameView.coinCounter.coinCount = 0;
        gameView.coinCounter.SetActive(false);

        gameView.batteryMeter.showBatteryButton = false;
        gameView.batteryMeter.SetActive(false);

        gameView.batteryCounter.batteryCount = 0;
        gameView.batteryCounter.SetActive(false);

        //FPSCounter fps = topLayer.AddChild(new FPSCounter());
        //fps.position = new Vector2((-group.size.x + fps.size.x + 10) * 0.5f, group.size.y * 0.25f);
    }

    IEnumerator _startGame()
    {
        if(!Player.exists)
        {
            Instantiate(playerPrefab);

            // spawn and animate player entrance
            Player.position = levelStart.spawnPosition.position;
            Player.rotation = levelStart.spawnPosition.rotation;
            
            CameraController.that.FollowPlayer(0);

            RenderManager.renderMode = CameraRenderMode.Colorful;
            RenderManager.FadeToColor(Color.clear, 1.0f);
            
            yield return new WaitForSeconds(0.5f);

            levelStart.doors.isOpen = true;

            yield return new WaitForSeconds(0.7f);

            Player.that.anim.CrossFade("Moving");

            yield return new WaitForSeconds(0.1f);

            Vector3 walkIn = levelStart.startPosition.position - levelStart.spawnPosition.position;
            float dist = walkIn.magnitude;
            walkIn /= dist;

            float length = (dist / (Difficulty.playerMoveSpeed * 0.7f));
            float t = 0;

            while(t < length)
            {
                t += Time.deltaTime;
                float tn = Mathf.Clamp01(t / length);

                Player.position = Vector3.Lerp(
                    levelStart.spawnPosition.position,
                    levelStart.startPosition.position,
                    tn);

                yield return null;
            }
            
            Player.position = levelStart.startPosition.position;
            Player.rotation = levelStart.startPosition.rotation;
            Player.that.body.velocity = Vector3.zero;
            Player.that.body.angularVelocity = Vector3.zero;
            
            levelStart.doors.isOpen = false;
        }
        else
        {
            RenderManager.FadeToColor(Color.clear, 0.1f);
        }

        Player.that.anim.CrossFade("Idle");

        if (isFirstLevel)
        {
            Player.that.plugHoleSparks.SetActive(true);
            Player.that.plugHoleSparks.Play(true);
            yield return new WaitForSeconds(3.5f);
        }

        CameraController.that.FollowPlayer();
        yield return new WaitForSeconds(0.5f);

        gameView.mainJoystick.SetActive(true);
        gameView.jumpArea.SetActive(true);
        gameView.gearCounter.SetActive(true);
        gameView.coinCounter.SetActive(true);
        gameView.batteryCounter.SetActive(true);
        gameView.batteryMeter.SetActive(true);

        if(isFirstLevel)
        {
            yield return new WaitForSeconds(0.8f);

            Player.that.plugHoleSparks.Stop();
            Player.that.plugHoleSparks.SetActive(false);

            SharedSounds.lowBattery.Play();
            gameView.batteryMeter.DoBarFlash(3);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            gameView.batteryMeter.DoBarFlash(3);
        }

        _levelIndex = Util.levelIndex;
        _startTime = Time.time;
        _isRunning = true;

        Player.that.enabled = true;
        onGameStart?.Invoke();

        StartGame();
    }
    
    IEnumerator _killPlayer()
    {
        _isRunning = false;
        _playerDead = true;

        bool shortCircuiting = Player.that.isShortCircuiting;

        IEnumerator shutDown = Player.that._shutDown(shortCircuiting);
        Player.that.StartCoroutine(shutDown);
        
        bool hasBattery = gameView.batteryCounter.batteryCount > 0;
        
        float flickerEnd = Time.time + 1.0f;
        float nextFlicker = 0.0f;
        bool isDark = false;
        
        while(Player.isBatteryDead && Time.time <= flickerEnd)
        {
            if(Time.time >= nextFlicker)
            {
                float flickerLength = UnityEngine.Random.value * 0.15f;
                float flickerShade = UnityEngine.Random.value * 0.5f + 0.5f;

                isDark = !isDark;

                Color col = isDark ? new Color(0, 0, 0, flickerShade) : Color.clear;
                RenderManager.FadeToColor(col, flickerLength);
                
                nextFlicker = Time.time + flickerLength;
            }

            yield return null;
        }

        if(hasBattery)
            gameView.batteryMeter.StartReviveArrow();

        RenderManager.FadeToColor(new Color(0, 0, 0, 0.7f), 0.2f);
        
        float now = Time.time;

        if(hasBattery)
        {
            float allowReviveUntil = now + 4.5f;

            while(Player.isBatteryDead && Time.time <= allowReviveUntil)
                yield return null;

            gameView.batteryMeter.StopReviveArrow();
        }
        else
        {
            yield return new WaitForSeconds(2.0f);
        }

        if(!Player.isBatteryDead)
        {
            RenderManager.FadeToColor(Color.clear, 0.5f);

            Player.that.StopCoroutine(shutDown);
            Player.that.StartCoroutine(Player.that._powerUp());
            Player.that.StartCoroutine(Player.that._makeInvincible(5));
            _playerDead = false;
            _isRunning = true;
        }
        else
        {
            StartCoroutine(_endFailure());
        }
    }

    IEnumerator _endSuccess()
    {
        _isRunning = false;
        Player.that.enabled = false;
        if(levelEnd.bigDoors) levelEnd.bigDoors.isOpen = false;
        gameView.batteryMeter.SetActive(false);
        gameView.mainJoystick.SetActive(false);
        gameView.jumpArea.SetActive(false);
        
        float completionTime = Mathf.Max(Time.time - _startTime, 0);
        int gearCount = gameView.gearCounter.gearCount;
        int coinCount = gameView.coinCounter.coinCount;
        int batteriesRemaining = gameView.batteryCounter.batteryCount;
        bool expertPassed = Difficulty.level == DifficultyLevel.Hard && gearCount == 3 && coinCount == 100;

        Commit(_levelIndex, completionTime, gearCount, coinCount, batteriesRemaining, expertPassed);
        
        MusicMixer.Stop(1.0f);
        yield return new WaitForSeconds(1.0f);

        gameView.gamePassView.Show(_levelIndex, gearCount, completionTime, coinCount, batteriesRemaining, expertPassed);
        
        yield break;
    }

    Dictionary<BatteryDrainReason, string> batteryDrainHints = new Dictionary<BatteryDrainReason, string>(){
        [BatteryDrainReason.None] = "You ran out of power.",
        [BatteryDrainReason.Usage] = "You ran out of power.\nFind a spare battery,\nor get into a charger.",
        [BatteryDrainReason.Water] = "You fried your circuits!\nStay out of the water.",
        [BatteryDrainReason.Lasers] = "You fried your circuits!\nDon't walk through lasers.",
        [BatteryDrainReason.Landmine] = "Stay away from plasma\nmines, a power surge may\ncause a short circuit.",
        [BatteryDrainReason.PlasmaShell] = "Keep moving to avoid\nplasma shells. They can\ncause a short circuit.",
        [BatteryDrainReason.TurretFire] = "Keep moving to avoid\nplasma shots. They can\ncause a short circuit.",
        [BatteryDrainReason.PowerSurge] = "Power surges will\ndestroy your circuits.\nAvoid electric arcs.",
    };

    IEnumerator _endFailure()
    {
        _isRunning = false;
        Player.that.enabled = false;
        gameView.batteryMeter.SetActive(false);
        gameView.mainJoystick.SetActive(false);
        gameView.jumpArea.SetActive(false);

        MusicMixer.Stop(1.0f);
        yield return new WaitForSeconds(1.0f);

        gameView.gameFailView.Show(_levelIndex, batteryDrainHints[Player.that.drainReason]);

        yield break;
    }
}
