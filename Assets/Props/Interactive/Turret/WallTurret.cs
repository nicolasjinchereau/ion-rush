using UnityEngine;
using System.Collections;

public class WallTurret : MonoBehaviour
{
    public Transform barrelParent;
    public Transform barrels;
    public ParticleSystem bullets;

    private float barrelSpeed = 0.0f;
    private float maxBarrelSpeed = 720.0f;
    private float trackingSpeed = 4.0f;
    private AudioSource fireSound;
    private float nextFire = 0.0f;

    void Awake()
    {
        fireSound = GetComponent<AudioSource>();
        fireSound.ignoreListenerVolume = true;
    }

    void Update()
    {
        if(!GameController.state.isRunning)
            return;

        Vector3 toPlayer = Player.that.transform.position - barrelParent.position + Vector3.up * 0.5f;
        float distance = toPlayer.magnitude;

        if(Player.that.enabled && distance <= Difficulty.turretRange)
        {
            Quaternion newRotation = Quaternion.LookRotation(toPlayer);

            barrelParent.transform.rotation = Quaternion.Slerp(barrelParent.transform.rotation, newRotation, Time.deltaTime * trackingSpeed);
            barrelSpeed = Mathf.Min(barrelSpeed + Time.deltaTime * maxBarrelSpeed, maxBarrelSpeed);
        }
        else
        {
            barrelSpeed = Mathf.Max(barrelSpeed - Time.deltaTime * maxBarrelSpeed, 0);
        }

        if(barrelSpeed > float.Epsilon)
        {
            barrels.transform.Rotate(Vector3.forward, barrelSpeed * Time.deltaTime, Space.Self);
            //barrels.transform.RotateAroundLocal(Vector3.forward, barrelSpeed * Time.deltaTime);
        }

        if(barrelSpeed > maxBarrelSpeed * 0.95f && Time.time >= nextFire)
        {
            fireSound.Play();
            bullets.Emit(1);
            nextFire = Time.time + 0.25f;
        }
    }
}
