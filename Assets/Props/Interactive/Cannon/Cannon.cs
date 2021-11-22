using UnityEngine;
using System.Collections;

public class Cannon : MonoBehaviour
{
    public Transform barrelParent;
    public Transform shellSpawn;
    public GameObject plasmaShellPrefab;
    public ParticleSystem splashParticles;
    public Transform innerBarrel;

    private float trackingSpeed = 4.0f;
    private AudioSource fireSound;
    private float nextFire = 0.0f;
    private float cooldown = 3.0f;
    private float gravity = 9.8f;
    private Vector3 innerBarrelStartPos;
    private float innerBarrelOffset = 0.15f;

    void Awake()
    {
        fireSound = GetComponent<AudioSource>();
        fireSound.ignoreListenerVolume = true;
        innerBarrelStartPos = innerBarrel.localPosition;

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PlasmaShell"), LayerMask.NameToLayer("Cannon"));
    }

    void Update()
    {
        if(!GameController.state.isRunning)
            return;

        var speed = Difficulty.cannonShellSpeed;

        Vector3 toPlayer = Player.that.transform.position - transform.position;
        toPlayer.y = 0;

        float distance = toPlayer.magnitude - 0.5f;

        var maxDist = (speed * speed) / gravity;
        var maxRange = maxDist + Difficulty.cannonDamageRadius + 1.8f;

        if (distance <= maxRange)
        {
            float a = Mathf.Clamp01(gravity * distance / (speed * speed));
            float angle = Mathf.Asin(a) / 2.0f;

            float x = Mathf.Cos(angle) * speed;
            float y = Mathf.Sin(angle) * speed;

            toPlayer = toPlayer.normalized * x;
            toPlayer.y = y;

            Quaternion newRotation = Quaternion.LookRotation(toPlayer);

            barrelParent.rotation = Quaternion.Slerp(shellSpawn.rotation, newRotation, Time.deltaTime * trackingSpeed);

            var now = Time.time;

            if(now > nextFire)
            {
                nextFire = now + cooldown;

                fireSound.Play();
                GameObject go = Instantiate(plasmaShellPrefab, shellSpawn.position, Quaternion.identity);
                PlasmaShell shell = go.GetComponent<PlasmaShell>();
                shell.velocity = toPlayer;
                DoSplash();

                innerBarrel.localPosition = innerBarrelStartPos + Vector3.forward * innerBarrelOffset;
            }
        }
        else
        {
            Vector3 vec = barrelParent.forward;
            vec.y = 0;

            Quaternion newRotation = Quaternion.LookRotation(vec);
            barrelParent.rotation = Quaternion.Slerp(barrelParent.rotation, newRotation, Time.deltaTime * 2.0f);
        }

        innerBarrel.localPosition = Vector3.Lerp(innerBarrel.localPosition, innerBarrelStartPos, Time.deltaTime * 3);
    }

    void DoSplash()
    {
        Vector3 pos = shellSpawn.position;

        for(int i = 0; i < 100; ++i)
        {
            Vector3 vel = Random.insideUnitSphere * 0.5f;

            if(vel.y < 0)
                vel.y = -vel.y;

            splashParticles.Emit(pos, vel, 0.6f, 0.3f, Color.white);
        }
    }

    void OnDrawGizmosSelected()
    {
        var speed = Difficulty.Easy.cannonShellSpeed;
        var maxRangeEasy = ((speed * speed) / gravity) + Difficulty.Easy.cannonDamageRadius + 1.8f;

        var oldColor = Gizmos.color;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxRangeEasy);
        Gizmos.color = oldColor;
    }
}
