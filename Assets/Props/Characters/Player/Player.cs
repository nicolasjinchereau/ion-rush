using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BatteryDrainReason
{
    None,
    Usage,
    Water,
    Lasers,
    Landmine,
    PlasmaShell,
    TurretFire,
    PowerSurge
}

public class Player : MonoBehaviour
{
    public bool drainBatteryOverTime = true;

    public static Player that { get; private set; }

    public static bool exists {
        get { return that != null; }
    }

    public static Vector3 position
    {
        get { return that.transform.position; }
        set { that.transform.position = that.body.position = value; }
    }
    
    public static Quaternion rotation
    {
        get { return that.transform.rotation; }
        set { that.transform.rotation = that.body.rotation = value; }
    }

    public const float TotalHeight = 1.35f;

    private Vector3 _direction = Vector3.zero;
    public static Vector3 direction {
        get { return that._direction; }
        set { that._direction = value; }
    }

    private float _battery = 100.0f;
    public static float battery
    {
        get { return that._battery; }
        set {
            that._battery = Mathf.Clamp(value, 0, 100);
            if(that._battery >= batteryWarningThreshold)
                that.didHaveLowBattery = false;
        }
    }

    public static bool isBatteryDead {
        get { return that._battery <= float.Epsilon; }
    }

    // if battery is dead, this is the reason for the last discharge
    private BatteryDrainReason _drainReason = BatteryDrainReason.None;
    public BatteryDrainReason drainReason {
        get { return _drainReason; }
    }

    private bool _useBattery = true;
    public static bool useBattery {
        get { return that._useBattery; }
        set { that._useBattery = value; }
    }
    
    private bool _takeDamage = true;
    public static bool takeDamage {
        get { return that._takeDamage; }
        set { that._takeDamage = value; }
    }

    bool _isGrounded = false;
    public static bool isGrounded {
        get { return that._isGrounded; }
    }

    public bool IsCharging {
        get { return isCharging; }
    }

    public bool isInWater {
        get { return inWater > 0; }
    }

    public bool isInLasers {
        get { return inLasers > 0; }
    }

    public bool isShortCircuiting {
        get { return inWater > 0 || inLasers > 0; }
    }

    const float flashSpeed = 2.0f;
    bool _isFlashing = false;
    bool _keepFlashing = false;
    float _flashTime = 0.0f;

    public bool IsFlashing
    {
        get { return _isFlashing; }
        set {
            if(value != _isFlashing)
            {
                _isFlashing = value;

                if(_isFlashing)
                {
                    if(!_keepFlashing)
                        _flashTime = 0;
                }
                else
                {
                    _keepFlashing = true;
                }
            }
        }
    }

    public GameObject plugHole;
    public Transform head;
    public Transform torso;
    public Transform leftShoulder;
    public Transform rightShoulder;
    public Transform leftElbow;
    public Transform rightElbow;
    public Transform wheel;
    public ParticleSystem sparksDie;
    public ParticleSystem sparksHit;
    public ParticleSystem dust;
    public ParticleSystem smoke;
    public ParticleSystem cordSparks;
    public ParticleSystem plugHoleSparks;
    public AudioSource powerUpSound;
    public AudioSource powerDownSound;
    public AudioSource moveSound;
    public CapsuleCollider col;
    public Rigidbody body;
    public Animation anim;
    public SlipstreamBadge slipstreamBadge;
    public ShadowCaster shadowCaster;
    public MeshRenderer[] meshRenderers;

    private const int batteryWarningThreshold = 25;
    private bool didHaveLowBattery = false;

    private int inWater = 0;
    private int inLasers = 0;

    private bool isCharging = false;
    private bool isInChargeField = false;
    [System.NonSerialized] public bool isOnGlowingFloor = false;

    private float nextWaterSpark = 0.0f;
    private float waterSparkEmitDelay = 0.28f;
    private float nextShortCircuitSound = 0;

    // layers
    [System.NonSerialized]
    public int playerLayer;
    private int notPlayerLayerMask;
    private int waterLayer;
    private int velocityAdderLayer;
    private int bulletsLayer;
    private int plasmaShellLayer;
    private int chargeLayer;
    private int chargeFieldLayer;
    private int laserLayer;

    // jumping
    public float playerTurnSpeed = 30.0f;
    public float maxSpeed = 4.0f;
    public float maxAirSpeed = 3.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20;
    const float extraJumpDist = 0.02f;
    const float maxJumpSlope = 45.0f;
    public float jumpTimeAllowance = 0.1f;
    float wheelRadius = 0;
    bool jumpPressed = false;
    bool didJump = false;
    Vector3 _lastDirection = Vector3.zero;
    float lastGrounded = 0;
    
    // Robot should never travel faster than this.
    const float clampSpeed = 20;

    public Material robotMaterial;
    
    Collider[] overlaps = new Collider[16];
    RaycastHit[] hits = new RaycastHit[16];

    private void Awake()
    {
        that = this;

        robotMaterial.SetOverrideTag("OcclusionType", "1");
        robotMaterial.SetFloat("_Highlight", 0);
        
        playerLayer = LayerMask.NameToLayer("Player");
        notPlayerLayerMask = ~(1 << playerLayer);
        waterLayer = LayerMask.NameToLayer("Water");
        velocityAdderLayer = LayerMask.NameToLayer("VelocityAdder");
        bulletsLayer = LayerMask.NameToLayer("Bullets");
        plasmaShellLayer = LayerMask.NameToLayer("PlasmaShell");
        chargeLayer = LayerMask.NameToLayer("Charger");
        chargeFieldLayer = LayerMask.NameToLayer("ChargeField");
        laserLayer = LayerMask.NameToLayer("Laser");

        wheelRadius = col.radius * col.transform.lossyScale.x * 0.99f;

        Physics.gravity = Vector3.down * gravity;

        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionY;
    }

    private void OnDestroy()
    {
        that = null;

        if(robotMaterial != null)
            robotMaterial.SetFloat("_Highlight", 0);
    }

    public void DoJump()
    {
        if(enabled)
            jumpPressed = true;
    }
    
    void Update()
    {
        //float mag = _direction.magnitude;
        //moveSound.pitch = mag;
        //moveSound.volume = 0.05f + (1.0f - Mathf.Pow(mag, 6)) * 0.05f;
        
        // update animations
        if(!didJump)
        {
            if(isCharging || isInChargeField)
            {
                anim.CrossFade("Charging");
            }
            else if(_direction.sqrMagnitude > float.Epsilon)
            {
                anim.CrossFade("Moving");
            }
            else
            {
                anim.CrossFade("Idle");
            }
        }

        // update battery
        if(isCharging || isInChargeField)
        {
            ChargeBattery(Difficulty.batteryChargeSpeed * Time.deltaTime);
        }
        else if(useBattery && drainBatteryOverTime)
        {
            DrainBattery(Difficulty.batteryDrainSpeed * Time.deltaTime, BatteryDrainReason.Usage);
        }

        if(inWater > 0)
        {
            DrainBattery(Difficulty.waterDrainSpeed * Time.deltaTime, BatteryDrainReason.Water);
            GameController.state.OnPlayerDamaged(3);
            if (Time.time >= nextWaterSpark)
            {
                nextWaterSpark = Time.time + waterSparkEmitDelay;

                Vector3 sparkPos = transform.position;
                sparkPos.y = 0.1f;

                Vector3 dir = Random.onUnitSphere;
                dir.y = Mathf.Abs(dir.y);

                sparksHit.transform.position = sparkPos;
                sparksHit.transform.rotation = Quaternion.LookRotation(dir);
                sparksHit.transform.position += sparksHit.transform.forward * 0.15f;
                sparksHit.Emit(7);

                dir = Random.onUnitSphere;
                dir.y = Mathf.Abs(dir.y);

                sparksDie.transform.position = sparkPos;
                sparksDie.transform.rotation = Quaternion.LookRotation(dir);
                sparksDie.transform.position += sparksDie.transform.forward * 0.15f;
                sparksDie.Emit(7);
            }

            if(Time.time >= nextShortCircuitSound)
            {
                SharedSounds.shortCircuit.Play();
                nextShortCircuitSound = Time.time + 0.8f;
            }
        }

        if(inLasers > 0)
        {
            DrainBattery(Difficulty.laserDrainSpeed * Time.deltaTime, BatteryDrainReason.Lasers);
            
            Vector3 sparkPos = transform.position;
            sparkPos.y = 0.4f;

            Vector3 dir = Random.onUnitSphere;
            sparksHit.transform.position = sparkPos;
            sparksHit.transform.rotation = Quaternion.LookRotation(dir);
            sparksHit.transform.position += sparksHit.transform.forward * 0.15f;
            sparksHit.Emit(7);

            dir = Random.onUnitSphere;
            sparksDie.transform.position = sparkPos;
            sparksDie.transform.rotation = Quaternion.LookRotation(dir);
            sparksDie.transform.position += sparksDie.transform.forward * 0.15f;
            sparksDie.Emit(7);

            if(Time.time >= nextShortCircuitSound)
            {
                SharedSounds.shortCircuit.Play();
                nextShortCircuitSound = Time.time + 0.8f;
            }
        }

        if(battery < batteryWarningThreshold)
        {
            if(!didHaveLowBattery)
            {
                SharedSounds.lowBattery.Play();
                didHaveLowBattery = true;
            }

            // maintain constant flash when battery is low
            GameController.state.OnPlayerDamaged(3);
        }
        else if(didHaveLowBattery)
        {
            didHaveLowBattery = false;
        }
        
        if(_isFlashing)
        {
            _flashTime += flashSpeed * Time.deltaTime;
            if(_flashTime > 1.0f)
                _flashTime = 0;
            
            float t = -Mathf.Cos(_flashTime * Mathf.PI * 2.0f) * 0.5f + 0.5f;
            robotMaterial.SetFloat("_Highlight", t);
        }
        else
        {
            if(_keepFlashing)
            {
                _flashTime += flashSpeed * Time.deltaTime;
                if(_flashTime > 1.0f)
                {
                    _flashTime = 0;
                    _keepFlashing = false;
                }
                
                float t = -Mathf.Cos(_flashTime * Mathf.PI * 2.0f) * 0.5f + 0.5f;
                robotMaterial.SetFloat("_Highlight", t);
            }
        }
    }
    
    public IEnumerator _shutDown(bool isDamaged)
    {
        this.enabled = false;

        anim.CrossFade("Shutdown");
        powerDownSound.Play();

        if(isDamaged)
        {
            EnableSmoke(true);

            for(int i = 0; i < 10; ++i)
            {
                sparksDie.transform.rotation = Quaternion.LookRotation(new Vector3(Random.value, 10.0f, Random.value), Vector3.right);
                sparksDie.Emit();

                yield return new WaitForSeconds(Random.value * 0.3f);
            }
        }
    }

    public IEnumerator _makeInvincible(float seconds)
    {
        takeDamage = false;
        IsFlashing = true;

        yield return new WaitForSeconds(seconds);
        
        IsFlashing = false;

        while(_keepFlashing)
            yield return null;

        takeDamage = true;
    }

    public IEnumerator _powerUp(bool enablePlayer = true)
    {
        EnableSmoke(false);

        powerUpSound.Play();
        powerUpSound.time = 0.1f;

        yield return new WaitForSeconds(0.2f);

        anim.CrossFade("Idle", 0.3f);
        yield return new WaitForSeconds(0.3f);
        
        this.enabled = enablePlayer;
    }

    [System.Obsolete]
    private IEnumerator _ShortCircuitAndDie()
    {
        //enabled = false;

        anim.CrossFade("Shutdown");
        powerDownSound.Play();

        EnableSmoke(true);

        for(int i = 0; i < 10; ++i)
        {
            sparksDie.transform.rotation = Quaternion.LookRotation(new Vector3(Random.value, 10.0f, Random.value), Vector3.right);
            sparksDie.Emit();

            if(battery > 0)
            {
                powerUpSound.Play();
                powerUpSound.time = 0.1f;

                anim.CrossFade("Idle");

                RenderManager.FadeToColor(Color.clear, 0.3f);

                yield return new WaitForSeconds(anim["Idle"].length);

                enabled = true;

                yield break;
            }

            yield return new WaitForSeconds(Random.value * 0.3f);
        }

        if(battery > 0)
        {
            powerUpSound.Play();

            anim.CrossFade("Idle");

            RenderManager.FadeToColor(Color.clear, 0.3f);

            yield return new WaitForSeconds(anim["Idle"].length);

            enabled = true;

            yield break;
        }

        RenderManager.FadeToColor(new Color(0.1f, 0.1f, 0.1f, 0.6f), 2.0f);

        yield return new WaitForSeconds(3.0f);
    }

    [System.Obsolete]
    private IEnumerator _DrainBatteryAndDie()
    {
        //enabled = false;

        anim.CrossFade("Shutdown");
        powerDownSound.Play();

        float length = 3.0f;
        float start = Time.time;
        float end = start + length;
        float nextFlicker = 0.0f;
        bool isDark = false;

        while(Time.time < end)
        {
            if(Time.time >= nextFlicker)
            {
                float flicker = Random.value * 0.15f;
                RenderManager.FadeToColor((isDark = !isDark) ? new Color(0, 0, 0, Random.value * 0.5f + 0.5f) : Color.clear, flicker);
                nextFlicker = Time.time + flicker;
            }

            if(battery > 0)
            {
                powerUpSound.Play();

                anim.CrossFade("Idle");

                RenderManager.FadeToColor(Color.clear, 0.3f);

                yield return new WaitForSeconds(anim["Idle"].length);

                enabled = true;

                yield break;
            }

            yield return null;
        }

        if(battery > 0)
        {
            powerUpSound.Play();

            anim.CrossFade("Idle");
            
            RenderManager.FadeToColor(Color.clear, 0.3f);

            yield return new WaitForSeconds(anim["Idle"].length);

            enabled = true;

            yield break;
        }

        RenderManager.FadeToColor(new Color(0, 0, 0, 0.7f), 0.2f);

        yield return new WaitForSeconds(1.0f);
    }

    Vector3 groundPoint
    {
        get
        {
            RaycastHit hit;
            return Physics.SphereCast(transform.position + Vector3.up * wheelRadius * 2.0f,
                                      wheelRadius,
                                      Vector3.down,
                                      out hit,
                                      100.0f,
                                      notPlayerLayerMask) ? hit.point : Vector3.zero;
        }
    }
    
    void FixedUpdate()
    {
        if(!didJump)
        {
            float oldLength = _lastDirection.magnitude;
            float newLength = _direction.magnitude;

            bool doPuff = false;

            if((newLength - oldLength) > 0.2f)
                doPuff = true;

            if(oldLength > 0.01f && newLength > 0.01f && Vector3.Angle(_lastDirection, _direction) > 60.0f)
                doPuff = true;

            if(doPuff)
                dust.Emit();
        }
        
        _lastDirection = _direction;

        // in case player falls through floor...
        if (body.position.y < -1.0f)
        {
            Debug.LogError("fell through floor");

            var pos = body.position;
            var rayOrigin = new Vector3(pos.x, 5.0f, pos.z);
            
            RaycastHit hit;
            if (Physics.SphereCast(rayOrigin, wheelRadius, Vector3.down, out hit, 5.0f, notPlayerLayerMask, QueryTriggerInteraction.Ignore))
            {
                body.velocity = Vector3.zero;
                body.position = hit.point;
                Debug.Log("corrected position");
            }
            else
            {
                Debug.LogError("surface for player not found");
            }
        }

        // MOVEMENT
        Vector3 start = body.position + Vector3.up * wheelRadius * 1.5f;
        float rayDist = wheelRadius * 0.5f + extraJumpDist;

        int overlapCount = Physics.OverlapSphereNonAlloc(start, wheelRadius, overlaps, notPlayerLayerMask, QueryTriggerInteraction.Ignore);
        while(overlapCount >= overlaps.Length)
        {
            overlaps = new Collider[overlaps.Length * 2];
            overlapCount = Physics.OverlapSphereNonAlloc(start, wheelRadius, overlaps, notPlayerLayerMask, QueryTriggerInteraction.Ignore);
        }
        
        int totalHitCount = Physics.SphereCastNonAlloc(start, wheelRadius, Vector3.down, hits, rayDist, notPlayerLayerMask, QueryTriggerInteraction.Ignore);
        while(totalHitCount >= hits.Length)
        {
            hits = new RaycastHit[hits.Length * 2];
            totalHitCount = Physics.SphereCastNonAlloc(start, wheelRadius, Vector3.down, hits, rayDist, notPlayerLayerMask, QueryTriggerInteraction.Ignore);
        }
        
        Vector3 groundVelocity = Vector3.zero;
        int hitCount = 0;
        
        bool grounded = false;
        
        for(int r = 0; r < totalHitCount; ++r)
        {
            var hit = hits[r];
            
            Collider hitCol = hit.collider;
            
            if(hitCol == col || hitCol.isTrigger)
                continue;
            
            bool embedded = false;
            
            for(int c = 0; c < overlapCount; ++c)
            {
                if(overlaps[c] == hitCol) {
                    embedded = true;
                    break;
                }
            }
            
            if(embedded)
                continue;
            
            Vector3 point = hit.point;
            bool canJump = Vector3.Angle(hit.normal, Vector3.up) < maxJumpSlope;
            if(canJump)
            {
                grounded = true;

                Rigidbody rb = hitCol.attachedRigidbody;

                if(rb != null && rb.isKinematic)
                {
                    groundVelocity += rb.GetPointVelocity(point);
                    ++hitCount;
                }
            }
        }
        
        if(_isGrounded)
            lastGrounded = Time.time;
        
        if(grounded && !_isGrounded && (Time.time - lastGrounded > 0.2f)) {
            SharedSounds.land.Play();
        }

        _isGrounded = grounded;
        
        if(_direction.magnitude > float.Epsilon)
        {
            Quaternion rot = Quaternion.Slerp(
                body.rotation,
                Quaternion.LookRotation(_direction.normalized),
                playerTurnSpeed * Time.fixedDeltaTime);

            body.MoveRotation(rot);
        }

        if(_isGrounded)
        {
            if(hitCount > 0)
                groundVelocity /= (float)hitCount;

            Vector3 vel = body.velocity;
            Vector3 velocity = Vector3.zero;

            if(_direction.magnitude > float.Epsilon)
            {
                velocity = _direction * maxSpeed + groundVelocity;
                velocity.y = vel.y;
            }
            else
            {
                velocity = groundVelocity;
                velocity.y = vel.y;
            }

            if(vel.y < 0)
                didJump = false;

            if(!didJump && jumpPressed && (Time.time - lastGrounded <= jumpTimeAllowance))
            {
                didJump = true;
                SharedSounds.jump.Play();
                anim["Jump"].time = 0.3f;
                anim.CrossFade("Jump");
                velocity.y = jumpSpeed;
            }
            
            body.velocity = Vector3.ClampMagnitude(velocity, clampSpeed);
        }
        else // airborn
        {
            Vector3 vel = body.velocity;

            if(_direction.magnitude > float.Epsilon)
            {
                Vector3 airDir = _direction.normalized;
                Vector3 fvel = new Vector3(vel.x, 0, vel.z);
                float extraSpeed = Mathf.Max(0.0f, Vector3.Dot(fvel, airDir));
                float speed = Mathf.Max(extraSpeed, maxAirSpeed);

                Vector3 f = airDir * speed;
                vel = new Vector3(f.x, vel.y, f.z);
            }
            else
            {
                float linearDampening = 0.5f;
                vel = vel * Mathf.Clamp01(1.0f - linearDampening * Time.fixedDeltaTime);
            }
            
            if(vel.y < 0)
                didJump = false;
            
            if(!didJump && jumpPressed && (Time.time - lastGrounded <= jumpTimeAllowance))
            {
                didJump = true;
                SharedSounds.jump.Play();
                anim["Jump"].time = 0.3f;
                anim.CrossFade("Jump");
                vel.y = jumpSpeed;
            }
            
            body.velocity = Vector3.ClampMagnitude(vel, clampSpeed);
        }
        
        jumpPressed = false;
    }
    
    void OnEnable() {
        body.constraints = RigidbodyConstraints.FreezeRotation;
        lastGrounded = Time.time;
    }

    void OnDisable()
    {
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionY;
        robotMaterial.SetFloat("_Highlight", 0);
    }

    public static void DoExplosiveDamage(Vector3 explosionPos, float damage, float radius, BatteryDrainReason reason)
    {
        if(!that.enabled)
            return;
        
        if(reason != BatteryDrainReason.Landmine && reason != BatteryDrainReason.PlasmaShell && reason != BatteryDrainReason.PowerSurge)
            Debug.LogError("Invalid DrainReason for explosive damage: " + reason);

        Vector3 myPos = that.col.ClosestPointOnBounds(explosionPos);

        float dist = (explosionPos - myPos).magnitude;
        
        if(dist <= radius)
        {
            GameController.state.OnPlayerDamaged(3);
            
            Vector3 sparkPos = Player.position + Vector3.up * 0.5f;

            that.sparksHit.transform.position = sparkPos;
            that.sparksHit.transform.LookAt(explosionPos);
            that.sparksHit.transform.position += that.sparksHit.transform.forward * 0.15f;
            that.sparksHit.Emit(4);

            that.sparksDie.transform.position = sparkPos;
            that.sparksDie.transform.LookAt(explosionPos);
            that.sparksDie.transform.position += that.sparksDie.transform.forward * 0.15f;
            that.sparksDie.Emit(3);
            
            float actualDamage = (1.0f - dist / radius) * damage;
            that.DrainBattery(actualDamage, reason);

            that.body.AddExplosionForce(actualDamage * 12, explosionPos, radius);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        int hitLayer = other.gameObject.layer;

        if(hitLayer == chargeLayer)
        {
            isCharging = true;
        }
        else if(hitLayer == chargeFieldLayer)
        {
            isInChargeField = true;
        }
        else if(hitLayer == waterLayer)
        {
            ++inWater;
        }
        else if(hitLayer == laserLayer)
        {
            ++inLasers;
        }
    }

    void OnTriggerExit(Collider other)
    {
        int hitLayer = other.gameObject.layer;

        if(hitLayer == chargeLayer)
        {
            isCharging = false;
        }
        else if(hitLayer == chargeFieldLayer)
        {
            isInChargeField = false;
        }
        else if(hitLayer == waterLayer)
        {
            --inWater;
        }
        else if(hitLayer == laserLayer)
        {
            --inLasers;
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if(other.layer == bulletsLayer)
        {
            int barFlashCount = 3;
            GameController.state.OnPlayerDamaged(barFlashCount);

            Vector3 sparkPos = transform.position;
            sparkPos.y = other.transform.position.y;

            sparksHit.transform.position = sparkPos;
            sparksHit.transform.LookAt(other.transform);
            sparksHit.transform.position += sparksHit.transform.forward * 0.15f;
            sparksHit.Emit(4);

            sparksDie.transform.position = sparkPos;
            sparksDie.transform.LookAt(other.transform);
            sparksDie.transform.position += sparksDie.transform.forward * 0.15f;
            sparksDie.Emit(3);

            DrainBattery(Difficulty.turretDamage, BatteryDrainReason.TurretFire);
        }
    }

    public static void EnableSmoke(bool enableSmoke, bool clear = false)
    {
        that.smoke.EnableEmission(enableSmoke);

        if(clear)
        {
            that.smoke.Clear();
        }
    }

    public void DrainBattery(float amount, BatteryDrainReason reason)
    {
        if(_takeDamage)
        {
            if(!isBatteryDead)
            {
                battery -= amount;

                if (isBatteryDead)
                    _drainReason = reason;
            }
        }
    }

    public void ChargeBattery(float amount)
    {
        battery += amount;
    }

    public static void Explode()
    {
        that.StartCoroutine(that._explode());
    }

    IEnumerator _explode()
    {
        yield break;
    }

    public void SetMaterial(Material material)
    {
        foreach (var rend in meshRenderers) {
            rend.material = material;
        }
    }
}
