using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinGear : MonoBehaviour
{
    public float speed = 30.0f;

	void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, speed * Time.time);
	}
}
