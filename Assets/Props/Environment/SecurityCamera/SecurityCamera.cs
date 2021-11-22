using UnityEngine;
using System.Collections;

public class SecurityCamera : MonoBehaviour
{
    public Transform movingPiece;

    float angle = 0.0f;
    int rotationDirection = 1;

    float maxRotation = 45.0f;
    float rotationDelay = 1.0f;
    float rotationSpeed = 25.0f;

    void Start()
    {
        angle = (Random.value * 2.0f - 1.0f) * maxRotation;
    }

    void Update()
    {
        if(rotationDirection > 0)
        {
            angle += Time.deltaTime * rotationSpeed;

            if(angle > maxRotation)
            {
                angle = maxRotation;
                rotationDirection = 0;
                StartCoroutine(PauseRotation(-1));
            }

            movingPiece.localRotation = Quaternion.Euler(0, 0, angle);
        }
        else if(rotationDirection < 0)
        {
            angle -= Time.deltaTime * rotationSpeed;

            if(angle < -maxRotation)
            {
                angle = -maxRotation;
                rotationDirection = 0;
                StartCoroutine(PauseRotation(1));
            }

            movingPiece.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    IEnumerator PauseRotation(int resumeDirection)
    {
        yield return new WaitForSeconds(rotationDelay);

        rotationDirection = resumeDirection;
    }

}
