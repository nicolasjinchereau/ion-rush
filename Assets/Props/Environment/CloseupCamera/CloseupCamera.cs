using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseupCamera : MonoBehaviour
{
    public MeshRenderer mesh;
    public Camera cam;

    float oldShakeAmp = 0;

    private void Awake()
    {
        Destroy(mesh);
        mesh = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            CameraController.that.ShowView(cam, 1.0f);
            oldShakeAmp = CameraController.that.CameraShakeAmplitude;
            CameraController.that.CameraShakeAmplitude = oldShakeAmp * 0.1f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            CameraController.that.FollowPlayer();
            CameraController.that.CameraShakeAmplitude = oldShakeAmp;
        }
    }
}
