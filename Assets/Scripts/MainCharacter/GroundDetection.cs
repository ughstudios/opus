using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetection : MonoBehaviour
{
	[SerializeField] bool _onGround;
	[SerializeField] Vector3 _bottomPos = Vector3.zero;
	[SerializeField] float _detectionRadius = 1f;
	[SerializeField] Color _gizmoColor = Color.red;
	[SerializeField] LayerMask _floorDetectionLayer;
	
	// Update is called once per frame
	void Update()
    {
		//This is for the floor detection
		var bPos = _bottomPos;

		bPos.x += transform.position.x;
		bPos.y += transform.position.y;
		bPos.z += transform.position.z;

		_onGround = Physics.CheckSphere(bPos, _detectionRadius, _floorDetectionLayer);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = _gizmoColor;

		var bPos = _bottomPos;

		bPos.x += transform.position.x;
		bPos.y += transform.position.y;
		bPos.z += transform.position.z;

		Gizmos.DrawWireSphere(bPos, _detectionRadius);
	}

	public bool OnGround
	{
		get => _onGround;
	}
}
