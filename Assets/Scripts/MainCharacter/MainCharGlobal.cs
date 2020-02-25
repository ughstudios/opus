using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MainCharGlobal : MonoBehaviour
{
	protected Rigidbody _rb;
	protected GroundDetection _groundDetection;

	protected virtual void Awake()
	{
		if (GetComponent<Rigidbody>() != null) _rb = GetComponent<Rigidbody>();
		if (GetComponent<GroundDetection>() != null) _groundDetection = GetComponent<GroundDetection>();
	}
}
