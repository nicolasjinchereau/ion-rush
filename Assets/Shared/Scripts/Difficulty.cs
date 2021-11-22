using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum DifficultyLevel : int
{
    Easy = 0,
    Hard = 1
}

public static class Difficulty
{
    public class Level
    {
        public float turretRange;
        public float turretDamage;
        public float cannonRange;
        public float cannonDamage;
        public float cannonDamageRadius;
        public float cannonShellSpeed;
        public float landMineRange;
        public float landMineArmingTime;
        public float landMineDamage;
        public float timedSurgeDamage;
        public float batteryChargeSpeed;
        public float batteryPickupCharge;
        public float batteryDrainSpeed;
        public float waterDrainSpeed;
        public float laserDrainSpeed;
        public float playerMoveSpeed;
        public int   bonusTimeLimit;
    }

    public static DifficultyLevel level = DifficultyLevel.Easy;

    public static float turretRange { get { return GetLevel(level).turretRange; } }
    public static float turretDamage { get { return GetLevel(level).turretDamage; } }
    public static float cannonRange { get { return GetLevel(level).cannonRange; } }
    public static float cannonDamage { get { return GetLevel(level).cannonDamage; } }
    public static float cannonDamageRadius { get { return GetLevel(level).cannonDamageRadius; } }
    public static float cannonShellSpeed { get { return GetLevel(level).cannonShellSpeed; } }
    public static float landMineRange { get { return GetLevel(level).landMineRange; } }
    public static float landMineArmingTime { get { return GetLevel(level).landMineArmingTime; } }
    public static float landMineDamage { get { return GetLevel(level).landMineDamage; } }
    public static float timedSurgeDamage { get { return GetLevel(level).timedSurgeDamage; } }
    public static float batteryChargeSpeed { get { return GetLevel(level).batteryChargeSpeed; } }
    public static float batteryPickupCharge { get { return GetLevel(level).batteryPickupCharge; } }
    public static float batteryDrainSpeed { get { return GetLevel(level).batteryDrainSpeed; } }
    public static float waterDrainSpeed { get { return GetLevel(level).waterDrainSpeed; } }
    public static float laserDrainSpeed { get { return GetLevel(level).laserDrainSpeed; } }
    public static float playerMoveSpeed { get { return GetLevel(level).playerMoveSpeed; } }
    public static int   bonusTimeLimit { get { return GetLevel(level).bonusTimeLimit; } }

    public static Level GetLevel(DifficultyLevel level) {
        return levels[(int)level];
    }

    public static Level Easy { get => GetLevel(DifficultyLevel.Easy); }
    public static Level Hard { get => GetLevel(DifficultyLevel.Hard); }

    private static Level[] levels = new Level[2]
    {
        new Level() {
            turretRange = 6.0f, // invariant
            turretDamage = 5.0f,
            cannonRange = 8.0f, // invariant
            cannonDamage = 10.0f,
            cannonDamageRadius = 0.5f, // invariant
            cannonShellSpeed = 4.0f, // invariant
            landMineRange = 2.5f, // invariant
            landMineArmingTime = 0.4f,
            landMineDamage = 50.0f,
            timedSurgeDamage = 50.0f,
            batteryChargeSpeed = 33.33333f,  // invariant
            batteryPickupCharge = 100.0f,  // invariant
            batteryDrainSpeed = 1.66666f,  // invariant
            waterDrainSpeed = 20.0f,
            laserDrainSpeed = 700.0f,
            playerMoveSpeed = 5.0f,  // invariant
            bonusTimeLimit = 180 // 3:00
        },
        new Level()
        {
            turretRange = 6.0f, // invariant
            turretDamage = 10.0f,
            cannonRange = 8.0f, // invariant
            cannonDamage = 20.0f,
            cannonDamageRadius = 0.5f, // invariant
            cannonShellSpeed = 4.0f, // invariant
            landMineRange = 2.5f, // invariant
            landMineArmingTime = 0.2f,
            landMineDamage = 100.0f,
            timedSurgeDamage = 100.0f,
            batteryChargeSpeed = 33.33333f, // invariant
            batteryPickupCharge = 100.0f, // invariant
            batteryDrainSpeed = 1.66666f, // invariant
            waterDrainSpeed = 40.0f,
            laserDrainSpeed = 1400.0f,
            playerMoveSpeed = 5.0f, // invariant
            bonusTimeLimit = 150 // 2:30
        },
    };
}
