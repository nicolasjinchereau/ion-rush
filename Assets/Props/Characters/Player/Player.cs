using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

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
    public Animation anim;
    public Rigidbody body;
    public Rigidbody wheelBody;
    public ConfigurableJoint wheelJoint;
    public Collider wheelCollider;
    public SlipstreamBadge slipstreamBadge;
    public ShadowCaster shadowCaster;
    public MeshRenderer[] meshRenderers;
    public Material robotMaterial;

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
    private int waterLayer;
    private int bulletsLayer;
    private int chargeLayer;
    private int chargeFieldLayer;
    private int laserLayer;

    const float MaxMoveSpeed = 4.0f;
    const float MaxAirSpeed = 4.0f;
    const float MaxJumpSlope = 45.0f;
    const float AngularVelocityFactor = 19.0f;
    const float JumpImpulse = 12.91f;
    const float JumpTimeAllowance = 0.1f;
    const float MinAirtimeBeforeLandingSound = 0.2f;

    bool grounded = true;
    bool groundedLastUpdate = true;
    bool jumpPressed = false;
    Vector3 lastDirection = Vector3.zero;
    float lastGroundedTime = float.MinValue;

    private void Awake()
    {
#if !UNITY_EDITOR
        drainBatteryOverTime = true;
#endif
        that = this;

        robotMaterial.SetOverrideTag("OcclusionType", "1");
        robotMaterial.SetFloat("_Highlight", 0);
        
        playerLayer = LayerMask.NameToLayer("Player");
        waterLayer = LayerMask.NameToLayer("Water");
        bulletsLayer = LayerMask.NameToLayer("Bullets");
        chargeLayer = LayerMask.NameToLayer("Charger");
        chargeFieldLayer = LayerMask.NameToLayer("ChargeField");
        laserLayer = LayerMask.NameToLayer("Laser");

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
        if(grounded)
        {
            if(isCharging || isInChargeField) {
                anim.CrossFade("Charging");
            }
            else if(_direction.sqrMagnitude > 10e-6f) {
                anim.CrossFade("Moving");
            }
            else {
                anim.CrossFade("Idle");
            }
        }

        UpdateBatteryState();
    }
    
    void FixedUpdate()
    {
        if(grounded)
        {
            float oldLength = lastDirection.magnitude;
            float newLength = _direction.magnitude;

            bool doPuff = false;

            if((newLength - oldLength) > 0.2f)
                doPuff = true;

            if(oldLength > 0.01f && newLength > 0.01f && Vector3.Angle(lastDirection, _direction) > 60.0f)
                doPuff = true;

            if(doPuff)
                dust.Emit();
        }
        
        lastDirection = _direction;

        // enable or disable wheel drive
        if(grounded)
        {
            var slerpDrive = wheelJoint.slerpDrive;
            slerpDrive.positionDamper = 10000;
            wheelJoint.slerpDrive = slerpDrive;
        }
        else
        {
            var slerpDrive = wheelJoint.slerpDrive;
            slerpDrive.positionDamper = 0;
            wheelJoint.slerpDrive = slerpDrive;
        }

        // update player body and wheel drive direction
        if (_direction.sqrMagnitude > 10e-6f)
        {
            var bodyRot = Quaternion.LookRotation(_direction.normalized, Vector3.up);
            body.MoveRotation(bodyRot);

            var worldAngularVelocity = new Vector3(-_direction.z, 0, _direction.x) * AngularVelocityFactor;
            var angularVelocity = wheelBody.rotation.Inverse() * worldAngularVelocity;
            wheelJoint.targetAngularVelocity = angularVelocity;
        }
        else
        {
            wheelJoint.targetAngularVelocity = Vector3.zero;
        }
        
        // add airborn propulsion
        if(!grounded)
        {
            Vector3 vel = body.velocity;

            if (_direction.sqrMagnitude > 10e-6f)
            {
                var inputSpeed = _direction.magnitude * MaxAirSpeed;
                var projectedSpeed = Mathf.Max(0.0f, Vector3.Dot(new Vector3(vel.x, 0, vel.z), _direction.normalized));
                var targetSpeed = Mathf.Max(inputSpeed, projectedSpeed);
                var targetVelocity = _direction.normalized * targetSpeed;
                vel = new Vector3(targetVelocity.x, vel.y, targetVelocity.z);
            }
            
            float linearDampening = 0.5f;
            float damp = Mathf.Clamp01(1.0f - linearDampening * Time.fixedDeltaTime);
            vel = new Vector3(vel.x * damp, vel.y, vel.z * damp);

            body.velocity = vel;
        }

        // do landing sound
        if(grounded && !groundedLastUpdate &&
            (Time.time - lastGroundedTime) >= MinAirtimeBeforeLandingSound)
        {
            SharedSounds.land.Play();
        }

        // do jump
        if(grounded)
            lastGroundedTime = Time.time;
        
        if(jumpPressed && (Time.time - lastGroundedTime) <= JumpTimeAllowance)
        {
            SharedSounds.jump.Play();
            anim["Jump"].time = 0.3f;
            anim.CrossFade("Jump");
            lastGroundedTime = float.MinValue;
            body.AddForce(Vector3.up * JumpImpulse, ForceMode.Impulse);
        }
        
        groundedLastUpdate = grounded;
        grounded = false;
        jumpPressed = false;
    }

    private void OnCollisionEnter(Collision collision) {
        UpdateGroundedState(collision);
    }

    private void OnCollisionStay(Collision collision) {
        UpdateGroundedState(collision);
    }

    private void UpdateGroundedState(Collision collision)
    {
        if (!grounded)
        {
            foreach(var contact in collision.contacts)
            {
                if(contact.thisCollider == wheelCollider)
                {
                    var floorSlope = Vector3.Angle(contact.normal, Vector3.up);
                    if (floorSlope < MaxJumpSlope)
                    {
                        grounded = true;
                        break;
                    }
                }
            }
        }
    }

    void OnEnable()
    {
        body.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void OnDisable()
    {
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionY;
        robotMaterial.SetFloat("_Highlight", 0);
    }

    void UpdateBatteryState()
    {
        if(isCharging || isInChargeField) {
            ChargeBattery(Difficulty.batteryChargeSpeed * Time.deltaTime);
        }
        else if(useBattery && drainBatteryOverTime) {
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

        if(hitLayer == chargeLayer) {
            isCharging = true;
        }
        else if(hitLayer == chargeFieldLayer) {
            isInChargeField = true;
        }
        else if(hitLayer == waterLayer) {
            ++inWater;
        }
        else if(hitLayer == laserLayer) {
            ++inLasers;
        }
    }

    void OnTriggerExit(Collider other)
    {
        int hitLayer = other.gameObject.layer;

        if(hitLayer == chargeLayer) {
            isCharging = false;
        }
        else if(hitLayer == chargeFieldLayer) {
            isInChargeField = false;
        }
        else if(hitLayer == waterLayer) {
            --inWater;
        }
        else if(hitLayer == laserLayer) {
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
            that.smoke.Clear();
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

    public void ChargeBattery(float amount) {
        battery += amount;
    }

    public void SetMaterial(Material material)
    {
        foreach (var rend in meshRenderers) {
            rend.material = material;
        }
    }
}
