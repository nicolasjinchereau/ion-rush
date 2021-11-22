using UnityEngine;
using System.Collections;

public class GoodRobot : Useable
{
    Vector3 direction = Vector3.zero;
    float wheelCircumference = 0.8206593f;

    public Transform leftFrontWheel;
    public Transform leftBackWheel;
    public Transform rightFrontWheel;
    public Transform rightBackWheel;

    public Rigidbody body;

    float robotSpeed = 2.0f;
    float robotTurnSpeed = 1.0f;
    int laserLayer = 0;
    int inLasers = 0;
    float nextLaserSparks = 0.0f;
    float nextSparkSound = 0.0f;

    public AudioSource movingSound;
    public AudioSource shortCircuitSound;
    public ParticleSystem sparks01;
    public ParticleSystem sparks02;

    RobotUseTrigger targetTrigger = null;

    public override void OnAwake() {
        base.OnAwake();
        laserLayer = LayerMask.NameToLayer("Laser");
    }

    void Start() {
        movingSound.ignoreListenerVolume = true;
    }
    
    void Update()
    {
        if(direction.sqrMagnitude > 0.01f)
        {
            direction.Normalize();
            Quaternion newRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * robotTurnSpeed);
            body.rotation = transform.rotation = newRotation;

            body.velocity = transform.forward * robotSpeed;

            float wheelRotatation = robotSpeed * Time.deltaTime * wheelCircumference * 360.0f;

            leftFrontWheel.localRotation = Quaternion.Euler(wheelRotatation, 0, 0) * leftFrontWheel.localRotation;
            leftBackWheel.localRotation = Quaternion.Euler(wheelRotatation, 0, 0) * leftFrontWheel.localRotation;
            rightFrontWheel.localRotation = Quaternion.Euler(wheelRotatation, 0, 0) * leftFrontWheel.localRotation;
            rightBackWheel.localRotation = Quaternion.Euler(wheelRotatation, 0, 0) * leftFrontWheel.localRotation;

            if(!movingSound.isPlaying)
                movingSound.Play();
        }
        else
        {
            if(movingSound.isPlaying)
                movingSound.Stop();

            body.velocity = Vector3.zero;
        }

        if(inLasers > 0)
        {
            if(Time.time >= nextLaserSparks)
            {
                Vector3 sparkPos = transform.position;
                sparkPos.y = 0.1f;

                Vector3 dir1 = Random.onUnitSphere;
                Vector3 dir2 = Random.onUnitSphere;
                if(dir1.y < 0) dir1 = -dir1;
                if(dir2.y < 0) dir2 = -dir2;

                sparks01.transform.position = sparkPos;
                sparks01.transform.rotation = Quaternion.LookRotation(dir1);
                sparks01.transform.position += sparks01.transform.forward * 0.5f;

                sparks02.transform.position = sparkPos;
                sparks02.transform.rotation = Quaternion.LookRotation(dir2);
                sparks02.transform.position += sparks02.transform.forward * 0.4f;
                
                nextLaserSparks = Time.time + 0.17f;
            }

            if(Time.time >= nextSparkSound)
            {
                shortCircuitSound.Play();
                nextSparkSound = Time.time + 0.8f;
            }
        }
    }
    
    public override void OnUseStart()
    {
    }

    public override void OnUseFinish()
    {
    }

    public override void OnUpdateDirection(Vector3 direction)
    {
        this.direction = direction;
    }

    public override int OnAction()
    {
        if(targetTrigger != null)
        {
            targetTrigger.DoAction();
            return 1;
        }
        else
        {
            return 0;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "RobotUseTrigger")
        {
            targetTrigger = other.GetComponent<RobotUseTrigger>();
            if(targetTrigger != null)
                targetTrigger.InUse = true;
            
            ready = true;
        }

        if(other.gameObject.layer == laserLayer)
        {
            if(inLasers++ == 0)
            {
                sparks01.EnableEmission(true);
                sparks02.EnableEmission(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "RobotUseTrigger")
        {
            var otherTarget = other.GetComponent<RobotUseTrigger>();
            if(targetTrigger = otherTarget)
            {
                targetTrigger.InUse = false;
                targetTrigger = null;
            }

            ready = false;
        }

        if(other.gameObject.layer == laserLayer)
        {
            if(--inLasers == 0)
            {
                sparks01.EnableEmission(false);
                sparks02.EnableEmission(false);
            }
        }
    }
}
