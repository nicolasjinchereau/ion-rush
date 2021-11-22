using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameView : ViewController
{
    public MainJoystick mainJoystick;
    public JumpArea jumpArea;
    public UsePanel usePanel;
    public ShortCircuitOverlay shortCircuit;
    public BonusGearCounter bonusGearCounter;
    public BonusTimer bonusTimer;
    public GearCounter gearCounter;
    public BatteryMeter batteryMeter;
    public CoinCounter coinCounter;
    public BatteryCounter batteryCounter;
    public Button backButton;
    public GamePassView gamePassView;
    public GameFailView gameFailView;
    public BonusGameResultView resultView;
}
