using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class GamePassView : MonoBehaviour
{
    public BasicGearCounter gearCounter;
    public CoinCounter coinCounter;
    public BatteryCounter batteryCounter;

    public RectTransform rectTransform;
    public TMP_Text titleText;
    public RectTransform timeArea;
    public RectTransform timeClock;
    public TMP_Text timeText;

    public RectTransform gearArea;
    public Image gear1a;
    public Image gear2a;
    public Image gear2b;
    public Image gear3a;
    public Image gear3b;
    public Image gear3c;

    public FloatingPoints floatingPoints;

    public RectTransform expertStar;
    public Image expertStarImage;
    public P2DParticleSystem expertStarParticles;
    public Image finalExpertStar;

    public StackLayout coinCountAreaStackLayout;
    public RectTransform coinCountArea;
    public P2DParticleSystem coinCountAreaParticles;
    
    public RectTransform coin;
    public P2DParticleSystem coinParticles;

    public TMP_Text coinCountText;
    public TMP_Text nextButtonText;

    public AudioClip thudSound;
    public AudioClip passSound;
    public AudioClip swooshSound;
    public AudioClip bigExplosionSound;

    public GameObject quitButton;
    public GameObject retryButton;
    public GameObject nextButton;

    GameObject oldSelection;

    int level;
    int gearCount;
    float completionTime;
    int coinCount;
    int batteriesRemaining;
    bool expertMode = false;

    float totalCoins = 0;
    float displayedCoins = 0;
    const float CoinAccumulationRate = 1000.0f; // per second

    const float CoinUpscaleAmount = 0.4f;
    float coinIconUpscale = 0; // [0, 1]

    int nextLevel;

    private void OnEnable()
    {
        oldSelection = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(nextButton);
    }

    private void OnDisable()
    {
        if(oldSelection)
            EventSystem.current.SetSelectedGameObject(oldSelection);
    }

    public void Show(int level, int gearCount, float completionTime, int coinCount, int batteriesRemaining, bool expertMode)
    {
        this.level = level;
        this.gearCount = gearCount;
        this.completionTime = completionTime;
        this.coinCount = coinCount;
        this.batteriesRemaining = batteriesRemaining;
        this.expertMode = expertMode;

        // level title and time text
        int cmins = (int)completionTime / 60;
        int csecs = (int)completionTime % 60;
        string time = $"{cmins}:{csecs:00}";
        titleText.text = $"LEVEL {level + 1} PASSED";
        titleText.transform.localScale = new Vector3(0, 0, 1);
        timeText.text = $"<color=#0F0>{time}</color>";
        timeClock.localScale = new Vector3(0, 0, 1);
        timeText.rectTransform.localScale = new Vector3(0, 1, 1);

        // expert star
        expertStarImage.SetActive(false);
        finalExpertStar.SetActive(false);

        // gears
        gear1a.SetActive(false);
        gear2a.SetActive(false);
        gear2b.SetActive(false);
        gear3a.SetActive(false);
        gear3b.SetActive(false);
        gear3c.SetActive(false);

        // coin count text
        coinCountText.text = "0";
        coinCountArea.localScale = new Vector3(1, 0, 1);

        // bottom buttons
        nextLevel = level + 1;

        if (nextLevel < Profile.Levels.Count)
        {
            nextButtonText.text = "NEXT";
        }
        else
        {
            nextLevel = -1;
            nextButtonText.text = "DONE";
        }

        gameObject.SetActive(true);

        Util.PlayClip(passSound, 0.4f);
        StartCoroutine(DoAnimation());
    }

    public void ShowDebug()
    {
        float debugCompletionTime = 78;
        int debugGearCount = 2;
        int debugCoinCount = 78;
        int debugBatteryCount = 2;
        bool expertMode = true;

        gearCounter.gearCount = debugGearCount;
        coinCounter.coinCount = debugCoinCount;
        batteryCounter.batteryCount = debugBatteryCount;

        Show(1, debugGearCount, debugCompletionTime, debugCoinCount, debugBatteryCount, expertMode);
    }

    class CoinSpawnData
    {
        public float start;
        public GameObject go;
    }

    IEnumerator DoAnimation()
    {
        // animate time (scale up)
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(Util.Blend(0.5f, t => {
            t = Curve.BounceIn(t);
            titleText.transform.localScale = new Vector3(t, t, 1);
        }, () => titleText.transform.localScale = Vector3.one));

        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(Util.Blend(0.8f, t => {
            var et = Curve.InElastic(Util.NormalizedClamp(t, 0.0f, 0.6f));
            var bt = Curve.BounceIn(Util.NormalizedClamp(t, 0.6f, 1.0f));
            timeClock.localScale = new Vector3(et, et, 1);
            timeText.rectTransform.localScale = new Vector3(bt, 1, 1);
        }));

        yield return new WaitForSeconds(1.0f);

        const float GearSwipeLength = 1.5f;
        const float GearCollapseLength = 0.3f;
        const float BatterySwipeLength = 1.5f;
        const float BatterySwipePause = 0.4f;

        var gearStartSize = new Vector2(96, 96);
        var gearEndSize = new Vector2(360, 360);

        var ParticlesPerGear = 20;

        if (gearCount == 1)
        {
            var gear1 = gearCounter.collectedGears[0];

            var startPos = gear1.transform.position;
            var endPos = gear1a.transform.position;

            gearCounter.collectedGears[0].SetActive(false);
            gearCounter.uncollectedGears[0].SetActive(true);
            gear1a.SetActive(true);

            gearCounter.gearParticles[0].Emit(ParticlesPerGear);

            StartCoroutine(Util.InvokeDelayed(1.0f, () => Util.PlayClip(swooshSound)));
            yield return StartCoroutine(Util.Blend(GearSwipeLength, t => {
                var p1 = Util.NormalizedClamp(t, 0.0f, 0.5f);
                var p2 = Util.NormalizedClamp(t, 0.5f, 0.7f);
                var p3 = Util.NormalizedClamp(t, 0.7f, 1.0f);
                p1 = Curve.InElastic(p1);
                p2 = Curve.InCube(p2);
                p3 = Curve.SmoothStepInSteep(p3);
                gear1a.rectTransform.localRotation = Quaternion.Euler(0, 0, t * 360.0f);
                gear1a.rectTransform.localScale = Vector3.one * Mathf.Lerp(Mathf.Lerp(1.0f, 1.3f, p1), 1.0f, p2);
                gear1a.rectTransform.sizeDelta = Vector2.Lerp(gearStartSize, gearEndSize, p3);
                gear1a.transform.position = Vector3.Lerp(startPos, endPos, p3);
            }));
        }
        else if (gearCount == 2)
        {
            var gear1 = gearCounter.collectedGears[0];
            var gear2 = gearCounter.collectedGears[1];
            
            var startPos1 = gear1.transform.position;
            var startPos2 = gear2.transform.position;
            var endPos1 = gear2a.transform.position;
            var endPos2 = gear2b.transform.position;

            gearCounter.collectedGears[0].SetActive(false);
            gearCounter.collectedGears[1].SetActive(false);
            gearCounter.uncollectedGears[0].SetActive(true);
            gearCounter.uncollectedGears[1].SetActive(true);
            gear2a.SetActive(true);
            gear2b.SetActive(true);

            gearCounter.gearParticles[0].Emit(ParticlesPerGear);
            gearCounter.gearParticles[1].Emit(ParticlesPerGear);

            StartCoroutine(Util.InvokeDelayed(1.0f, () => Util.PlayClip(swooshSound)));
            yield return StartCoroutine(Util.Blend(GearSwipeLength, t => {
                var p1 = Util.NormalizedClamp(t, 0.0f, 0.5f);
                var p2 = Util.NormalizedClamp(t, 0.5f, 0.7f);
                var p3 = Util.NormalizedClamp(t, 0.7f, 1.0f);
                p1 = Curve.InElastic(p1);
                p2 = Curve.InCube(p2);
                p3 = Curve.SmoothStepInSteep(p3);

                gear2a.rectTransform.localScale = Vector3.one * Mathf.Lerp(Mathf.Lerp(1.0f, 1.3f, p1), 1.0f, p2);
                gear2a.rectTransform.sizeDelta = Vector2.Lerp(gearStartSize, gearEndSize, p3);
                gear2a.transform.position = Vector3.Lerp(startPos1, endPos1, p3);

                gear2b.rectTransform.localScale = Vector3.one * Mathf.Lerp(Mathf.Lerp(1.0f, 1.3f, p1), 1.0f, p2);
                gear2b.rectTransform.sizeDelta = Vector2.Lerp(gearStartSize, gearEndSize, p3);
                gear2b.transform.position = Vector3.Lerp(startPos2, endPos2, p3);
            }));
        }
        else if (gearCount == 3)
        {
            var gear1 = gearCounter.collectedGears[0];
            var gear2 = gearCounter.collectedGears[1];
            var gear3 = gearCounter.collectedGears[2];

            var startPos1 = gear1.transform.position;
            var startPos2 = gear2.transform.position;
            var startPos3 = gear3.transform.position;
            var endPos1 = gear3a.transform.position;
            var endPos2 = gear3b.transform.position;
            var endPos3 = gear3c.transform.position;

            gearCounter.collectedGears[0].SetActive(false);
            gearCounter.collectedGears[1].SetActive(false);
            gearCounter.collectedGears[2].SetActive(false);
            gearCounter.uncollectedGears[0].SetActive(true);
            gearCounter.uncollectedGears[1].SetActive(true);
            gearCounter.uncollectedGears[2].SetActive(true);
            gear3a.SetActive(true);
            gear3b.SetActive(true);
            gear3c.SetActive(true);

            gearCounter.gearParticles[0].Emit(ParticlesPerGear);
            gearCounter.gearParticles[1].Emit(ParticlesPerGear);
            gearCounter.gearParticles[2].Emit(ParticlesPerGear);

            StartCoroutine(Util.InvokeDelayed(1.0f, () => Util.PlayClip(swooshSound)));
            yield return StartCoroutine(Util.Blend(GearSwipeLength, t => {
                var p1 = Util.NormalizedClamp(t, 0.0f, 0.5f);
                var p2 = Util.NormalizedClamp(t, 0.5f, 0.7f);
                var p3 = Util.NormalizedClamp(t, 0.7f, 1.0f);
                p1 = Curve.InElastic(p1);
                p2 = Curve.InCube(p2);
                p3 = Curve.SmoothStepInSteep(p3);

                gear3a.rectTransform.localScale = Vector3.one * Mathf.Lerp(Mathf.Lerp(1.0f, 1.3f, p1), 1.0f, p2);
                gear3a.rectTransform.sizeDelta = Vector2.Lerp(gearStartSize, gearEndSize, p3);
                gear3a.transform.position = Vector3.Lerp(startPos1, endPos1, p3);

                gear3b.rectTransform.localScale = Vector3.one * Mathf.Lerp(Mathf.Lerp(1.0f, 1.3f, p1), 1.0f, p2);
                gear3b.rectTransform.sizeDelta = Vector2.Lerp(gearStartSize, gearEndSize, p3);
                gear3b.transform.position = Vector3.Lerp(startPos2, endPos2, p3);

                gear3c.rectTransform.localScale = Vector3.one * Mathf.Lerp(Mathf.Lerp(1.0f, 1.3f, p1), 1.0f, p2);
                gear3c.rectTransform.sizeDelta = Vector2.Lerp(gearStartSize, gearEndSize, p3);
                gear3c.transform.position = Vector3.Lerp(startPos3, endPos3, p3);
            }));
        }

        yield return new WaitForSeconds(0.5f);

        var gearAreaStartPos = coinCountArea.position;
        var gearAreaEndPos = coin.position;

        // collapse gears into the coins
        if (gearCount != 0)
        {
            StartCoroutine(Util.Blend(GearCollapseLength, t => {
                t = Curve.InCube(t);
                gearArea.localScale = Vector3.one * Mathf.Lerp(1.0f, 0.0f, t);
                gearArea.position = Vector3.Lerp(gearAreaStartPos, gearAreaEndPos, t);
            }, ()=> {
                Util.PlayClip(bigExplosionSound);
                gearArea.SetActive(false);
                totalCoins += gearCount * Defaults.CoinsPerGear;
                HitCoin(30);
                SpawnFloatingPoints(Defaults.CoinsPerGear * gearCount, gearArea.transform.position);
            }));
        }

        yield return StartCoroutine(Util.Blend(0.3f, t => {
            t = Util.NormalizedClamp(t, 0.5f, 1.0f);
            t = Curve.InCube(t);
            coinCountArea.localScale = new Vector3(1, t, 1);
        }));

        yield return new WaitForSeconds(0.5f);

        if (coinCount != 0)
        {
            coinCounter.particles.Emit(20);

            StartCoroutine(Util.Blend(1.0f, t =>
            {
                var p1 = Util.NormalizedClamp(t, 0.0f, 0.8f);
                var p2 = Util.NormalizedClamp(t, 0.8f, 1.0f);
                p1 = Curve.InElastic(p1);
                p2 = Curve.InCube(p2);
                coinCounter.coinIcon.rectTransform.localScale = Vector3.one * Mathf.Lerp(Mathf.Lerp(1.0f, 1.3f, p1), 1.0f, p2);
            }));

            yield return new WaitForSeconds(1.0f);
        }

        Vector2 coinsStart = coinCounter.coinIcon.transform.position;
        Vector2 coinsFinish = coin.transform.position;

        const float CoinTravelTime = 0.2f;
        const float CoinAccumulationTime = 3.0f;
        float coinDispatchDelay = CoinAccumulationTime / coinCount;
        float coinTime = 0;
        float realTimeStart = Time.time;
        int coinsDispatched = 0;

        var coinData = new List<CoinSpawnData>(coinCount);
        float nextCoinSound = 0;

        while (true)
        {
            var now = Time.time;
            float elapsed = now - realTimeStart;

            // spawn as many coins as needed this frame to catch up
            while (coinsDispatched != coinCount && coinTime < elapsed)
            {
                GameObject coin = Instantiate(coinCounter.coinIcon.gameObject, transform);
                coinData.Add(new CoinSpawnData() { start = coinTime, go = coin });
                coinCounter.coinCount -= 1;
                coinTime += coinDispatchDelay;
                ++coinsDispatched;
            }

            bool hasMoreCoins = false;
            bool didHitCoin = false;
            
            // move all coins along their trajectory, and destroy if reached target
            foreach (var data in coinData)
            {
                if (data.go)
                {
                    var coinElapsed = (elapsed - data.start) / CoinTravelTime;
                    if (coinElapsed < 1.0f)
                    {
                        var t = Curve.InCube(coinElapsed);
                        data.go.transform.position = Vector3.Lerp(coinsStart, coinsFinish, t);
                        hasMoreCoins = true;
                    }
                    else
                    {
                        didHitCoin = true;
                        HitCoin(1);
                        totalCoins += 1;
                        Destroy(data.go);
                        data.go = null;
                    }
                }
            }

            if (didHitCoin && Time.time >= nextCoinSound)
            {
                SharedSounds.coinPickup.Play();
                nextCoinSound = Time.time + 0.1f;
            }

            if (!hasMoreCoins && coinsDispatched == coinCount)
                break;

            yield return null;
        }

        SpawnFloatingPoints(coinData.Count, coin.transform.position);

        coinData = null;

        yield return new WaitForSeconds(0.5f);

        Vector2 batteryStart = batteryCounter.batteryIcon.transform.position;
        Vector2 batteryFinish = coin.position;

        // swipe batteries into the points
        for (int i = 0; i != batteriesRemaining; ++i)
        {
            GameObject battery = Instantiate(batteryCounter.batteryIcon.gameObject, transform);
            var batteryIcon = battery.GetComponent<Image>();
            var batteryStartSize = batteryIcon.rectTransform.sizeDelta;

            batteryCounter.particles.Emit(20);

            yield return StartCoroutine(Util.Blend(BatterySwipeLength, t => {
                var p1 = Util.NormalizedClamp(t, 0.0f, 0.5f);
                var p2 = Util.NormalizedClamp(t, 0.5f, 0.7f);
                var p3 = Util.NormalizedClamp(t, 0.7f, 1.0f);
                p1 = Curve.InElastic(p1);
                p2 = Curve.InCube(p2);
                p3 = Curve.SmoothStepInSteep(p3);
                batteryIcon.rectTransform.localScale = Vector3.one * Mathf.Lerp(Mathf.Lerp(1.0f, 1.3f, p1), 1.0f, p2);
                batteryIcon.rectTransform.sizeDelta = Vector2.Lerp(batteryStartSize, batteryStartSize * 3, p3);
                batteryIcon.transform.position = Vector3.Lerp(batteryStart, batteryFinish, p3);
            }, () => {
                SharedSounds.useBattery.Play();
                batteryCounter.batteryCount -= 1;
                HitCoin(20);
                SpawnFloatingPoints(Defaults.CoinsPerBattery, coin.transform.position);
                totalCoins += Defaults.CoinsPerBattery;
                Destroy(battery);
            }));

            yield return new WaitForSeconds(BatterySwipePause);
        }

        yield return new WaitForSeconds(0.5f);

        if (expertMode)
        {
            var x = (rectTransform.rect.width + expertStarImage.rectTransform.sizeDelta.x) * 0.5f;
            expertStarImage.SetActive(true);
            expertStarImage.rectTransform.anchoredPosition = new Vector2(-680, 0);
            expertStarImage.rectTransform.localScale = Vector3.zero;
            
            yield return StartCoroutine(Util.Blend(0.5f, t => {
                t = Curve.OutCubeInv(t);
                expertStarImage.rectTransform.localScale = new Vector3(t, t, 1);
            }));

            expertStarParticles.transform.position = expertStarImage.transform.position;
            expertStarParticles.Emit(20);

            var len = SharedSounds.hint.clip.length;
            var swipeLen = 1.0f;
            var preDelay = swipeLen * 0.5f - len * 0.5f;
            var postDelay = swipeLen - preDelay;

            yield return new WaitForSeconds(1.0f);

            StartCoroutine(Util.Blend(swipeLen, t => {
                var sst = Curve.SmoothStepInSteep(t);
                var ipit = Curve.InPowerInv(t, 10);

                expertStarImage.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(-680, 680, sst), 0);
                expertStarImage.rectTransform.localScale = new Vector3(ipit, ipit, 1);
            }, () => Util.PlayClip(thudSound)));

            yield return new WaitForSeconds(preDelay);

            SharedSounds.hint.Play();
            var oldTotal = totalCoins;
            totalCoins *= Defaults.CoinExpertMultiplier;
            var diff = Mathf.RoundToInt(totalCoins - oldTotal);
            SpawnFloatingPoints(diff, coinCountArea.transform.position);
            coinCountAreaParticles.transform.position = coinCountArea.transform.position;
            coinCountAreaParticles.rectTransform.sizeDelta = coinCountArea.sizeDelta;
            coinCountAreaParticles.Emit(30);

            yield return new WaitForSeconds(postDelay);

            expertStar.SetActive(false);
            finalExpertStar.SetActive(true);

            yield return StartCoroutine(Util.Blend(0.7f, t => {
                t = Curve.InElastic(t);
                finalExpertStar.rectTransform.localScale = new Vector3(t, t, 1);
            }));
        }

        yield return null;
    }
    
    public void SpawnFloatingPoints(int points, Vector3 position)
    {
        var go = Instantiate(floatingPoints.gameObject, floatingPoints.transform.parent);
        var fp = go.GetComponent<FloatingPoints>();
        fp.transform.position = position;
        fp.label.text = $"+{points}";
        fp.gameObject.SetActive(true);
    }

    public void HitCoin(int particleCount = 0)
    {
        if (particleCount > 0)
        {
            coinParticles.transform.position = coin.position;
            coinParticles.rectTransform.sizeDelta = coin.sizeDelta;
            coinParticles.Emit(particleCount);
        }

        coinIconUpscale = 1.0f;
    }

    private void Update()
    {
        if (displayedCoins < totalCoins)
        {
            displayedCoins = Mathf.Min(displayedCoins + Time.deltaTime * CoinAccumulationRate, totalCoins);
            coinCountText.text = Mathf.RoundToInt(displayedCoins).ToString();
            coinCountText.rectTransform.UsePreferredWidth();
            coinCountAreaStackLayout.SetLayoutHorizontal();
        }
        
        float coinScale = 1.0f + coinIconUpscale * CoinUpscaleAmount;
        coinIconUpscale = Mathf.Max(coinIconUpscale - Time.deltaTime * 5.0f, 0);
        coin.localScale = Vector3.one * coinScale;
    }

    public void OnQuitPressed()
    {
        MusicMixer.Stop(0.3f);
        GameController.SetLevel(-1);
    }

    public void OnRetryPressed()
    {
        MusicMixer.Stop(0.3f);
        GameController.SetLevel(level);
    }

    public void OnNextPressed()
    {
        MusicMixer.Stop(0.3f);
        GameController.SetLevel(nextLevel);
    }
}
