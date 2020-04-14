using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleMoveTest : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		GetComponent<Rigidbody>().AddForce(Vector3.forward * 25, ForceMode.Impulse);
	}
}
