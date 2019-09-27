using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{

    public float degPerSec = 45;
	
	void Update ()
    {
        transform.Rotate(0f, degPerSec * Time.deltaTime, 0f);
	}
}
