using UnityEngine;
using System.Collections;

public class BadRobot : MonoBehaviour
{
    public Transform wheelFR;
    public Transform wheelFL;
    public Transform wheelBR;
    public Transform wheelBL;
    public float speed = 0.0f;
    float frontWheelCirc = 0;
    float backWheelCirc = 0;
    public AudioSource robotHit;
    public AudioSource rollingWheels;

    void Awake()
    {
        frontWheelCirc = Mathf.PI * wheelFR.GetComponent<Renderer>().bounds.size.y;
        backWheelCirc = Mathf.PI * wheelBR.GetComponent<Renderer>().bounds.size.y;
    }
    
    void Update()
    {
        if(speed > 0)
        {
            if(!rollingWheels.isPlaying)
                rollingWheels.Play();
        }
        else
        {
            if(rollingWheels.isPlaying)
                rollingWheels.Stop();
        }

        //transform.position += transform.forward * speed * Time.deltaTime;

        Quaternion frontRot = Quaternion.Euler(speed / frontWheelCirc * 360.0f * Time.deltaTime, 0, 0);
        Quaternion backRot = Quaternion.Euler(speed / backWheelCirc * 360.0f * Time.deltaTime, 0, 0);

        wheelFR.transform.localRotation = frontRot * wheelFR.transform.localRotation;
        wheelFL.transform.localRotation = frontRot * wheelFL.transform.localRotation;
        wheelBR.transform.localRotation = backRot * wheelBR.transform.localRotation;
        wheelBL.transform.localRotation = backRot * wheelBL.transform.localRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
            robotHit.Play();
    }
}
