using UnityEngine;
using System.Collections;

public class GroundTurret : MonoBehaviour
{
    public Transform barrelParent;
    public Transform barrels;
    public ParticleSystem bullets;
    public ShortCircuit brokenShortCircuit;
    public Collider mainCollider;

    public bool broken = false;
    public float rogueFireCycleLength = 3.0f;
    public float rogueFireCycleDelay = 4.0f;
    public float rogueFireInitialDelay = 0.0f;

    private float barrelSpeed = 0.0f;
    private float maxBarrelSpeed = 720.0f;
    private float trackingSpeed = 4.0f;
    private AudioSource fireSound;
    private float nextFire = 0.0f;
    private bool isFiring = false;

    Plane[] frustumPlanes = new Plane[6];

    void Awake()
    {
        fireSound = GetComponent<AudioSource>();
        fireSound.ignoreListenerVolume = true;
    }

    private void Start()
    {
        if(broken)
            nextFire = Time.time + rogueFireInitialDelay;
    }

    void Update()
    {
        if(!GameController.state.isRunning)
            return;

        if (!broken)
        {
            Vector3 toPlayer = Player.that.transform.position + Vector3.up * 0.5f - barrelParent.position;
            float distance = toPlayer.magnitude;

            if (Player.that.enabled && distance <= Difficulty.turretRange)
            {
                Quaternion newRotation = Quaternion.LookRotation(toPlayer);
                var euler = newRotation.eulerAngles;
                if (euler.x > 180) euler.x -= 360.0f;
                euler.x = Mathf.Clamp(euler.x, -60.0f, 47.0f);
                newRotation = Quaternion.Euler(euler);

                barrelParent.rotation = Quaternion.Slerp(barrelParent.rotation, newRotation, Time.deltaTime * trackingSpeed);
                barrelSpeed = Mathf.Min(barrelSpeed + Time.deltaTime * maxBarrelSpeed, maxBarrelSpeed);
            }
            else
            {
                barrelSpeed = Mathf.Max(barrelSpeed - Time.deltaTime * maxBarrelSpeed, 0);
            }

            if (barrelSpeed > float.Epsilon)
            {
                barrels.Rotate(Vector3.forward, barrelSpeed * Time.deltaTime, Space.Self);
            }

            if (barrelSpeed > maxBarrelSpeed * 0.95f && Time.time >= nextFire)
            {
                fireSound.Play();
                bullets.Emit(1);
                nextFire = Time.time + 0.25f;
            }
        }
        else
        {
            if (Time.time >= nextFire)
            {
                GeometryUtility.CalculateFrustumPlanes(CameraController.that.cam, frustumPlanes);
                var visible = GeometryUtility.TestPlanesAABB(frustumPlanes, mainCollider.bounds);
                if (visible)
                {
                    SharedSounds.shortCircuit.Play(0.2f);
                    brokenShortCircuit.Play();
                    StartCoroutine(DoMalfunctionFireCycle());
                }

                nextFire = Time.time + rogueFireCycleLength + rogueFireCycleDelay;
            }

            if (broken && !isFiring)
            {
                barrelParent.localRotation = Quaternion.Euler(30.0f, 0, 0);
            }
        }
    }
    
    IEnumerator DoMalfunctionFireCycle()
    {
        isFiring = true;

        var spinUpTime = 0.2f;
        var fireTime = rogueFireCycleLength - 2.0f * spinUpTime;

        var downRot = Quaternion.Euler(30.0f, 0, 0);
        var upRot = Quaternion.identity;

        yield return StartCoroutine(Util.Blend(spinUpTime, t => {
            barrelParent.localRotation = Quaternion.Slerp(downRot, upRot, Curve.OutQuadInv(t));
            barrelSpeed = Mathf.Lerp(0, maxBarrelSpeed * 2.0f, t);
            barrels.Rotate(Vector3.forward, barrelSpeed * Time.deltaTime, Space.Self);
        }));

        StartCoroutine(Util.Blend(fireTime, t => {
            barrels.Rotate(Vector3.forward, barrelSpeed * Time.deltaTime, Space.Self);
        }));

        var fireDone = Time.time + fireTime;
        while (Time.time < fireDone)
        {
            fireSound.Play();
            bullets.Emit(1);

            if (Time.time + 0.2f >= fireDone)
                break;
            
            yield return new WaitForSeconds(0.2f);
        }

        yield return StartCoroutine(Util.Blend(spinUpTime, t => {
            barrelParent.localRotation = Quaternion.Slerp(upRot, downRot, Curve.InQuad(t));
            barrelSpeed = Mathf.Lerp(maxBarrelSpeed * 2.0f, 0, t);
            barrels.Rotate(Vector3.forward, barrelSpeed * Time.deltaTime, Space.Self);
        }));

        isFiring = false;
    }

    void OnDrawGizmosSelected()
    {
        var oldColor = Gizmos.color;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(barrelParent.position, Difficulty.Easy.turretRange);
        Gizmos.color = oldColor;
    }
}
