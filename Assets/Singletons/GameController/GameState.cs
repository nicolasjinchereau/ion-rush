using UnityEngine;
using System.Collections;
using System;

public enum LevelResult
{
    Win,
    Loss,
    Quit,
}

public abstract class GameState : MonoBehaviour
{
    public bool isDefaultState = false;

    public void Awake() {
        enabled = false;
    }


    public virtual void PreStart(object[] args){}
    public virtual void Startup(object[] args){}
    public virtual void Shutdown(){}

    public virtual void DoPreStart(object[] args) {
        PreStart(args);
    }

    public virtual void DoStartup(object[] args) {
        Startup(args);
    }

    public virtual void DoShutdown() {
        this.Shutdown();
        Resources.UnloadUnusedAssets();
    }

    //public virtual UI.Group canvas { get { return null; } }
    public virtual bool isRunning { get { return false; } }
    
    // gameplay interface
    public virtual void OnUseTriggerActivated(UseTrigger trigger){}
    public virtual void OnUseTriggerDeactivated(UseTrigger trigger){}
    public virtual void OnPlayerDamaged(int amount){}
    public virtual void OnCoinsCollected(int count, Vector3? fromWorldPosition){}
    public virtual void OnGearsCollected(Vector3 fromWorldPosition){}
    public virtual void OnBatteriesCollected(int count, Vector3? fromWorldPosition){}
    public virtual void UseBatteries(int count){}
    
    public void StartGame()
    {

    }

    public void FinishGame(LevelResult result, int gearsCollected, int coinsCollected)
    {

    }

    public static void Commit(int level, float completionTime, int gearCount, int coinCount, int batteriesRemaining, bool expertPassed)
    {
        // update completion time
        float oldCompletionTime = Profile.Levels[level].completionTime;
        if(oldCompletionTime < 1.0f || completionTime < oldCompletionTime)
            Profile.Levels[level].completionTime = completionTime;

        // update coins collected
        if (coinCount > Profile.Levels[level].coinsCollected)
            Profile.Levels[level].coinsCollected = coinCount;

        // update batteries remaining
        if (batteriesRemaining > Profile.Levels[level].batteriesRemaining)
            Profile.Levels[level].batteriesRemaining = batteriesRemaining;

        // update gears collected
        if (gearCount > Profile.Levels[level].gearsCollected)
            Profile.Levels[level].gearsCollected = gearCount;

        // update expert status
        if (expertPassed)
            Profile.Levels[level].expertPassed = true;

        // unlock next level
        if((level + 1) < Profile.Levels.Count)
            Profile.Levels[level + 1].unlocked = true;

        // save settings to file
        Profile.Save();
    }
}
