using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using BeardedManStudios.Forge.Networking.Generated;

public class PlayerCamera : MonoBehaviour
{
	private float m_Tilt_X_Angle;                    // The rig's y axis rotation.
	[Range(0f, 10f)] [SerializeField] private float m_TurnSpeed = 1.5f;   // How fast the rig will rotate from user input.
	private Quaternion m_TransformTargetRot;
	[SerializeField] bool _inverse = false;
	[SerializeField] float	m_TiltMin = 45.0f,
							m_TiltMax = 45.0f;

	[SerializeField] GameObject _firePosition = null;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}


	void Update()
    {
		HandleXRotation();

		var x = CrossPlatformInputManager.GetAxis("Mouse X");
		transform.parent.gameObject.transform.Rotate(Vector3.up * x);

		//_firePosition.transform.rotation = transform.rotation;
	}

	void HandleXRotation()
	{
		// Read the user input, we only need the Y axis because the cam is following the player rot
		var y = CrossPlatformInputManager.GetAxis("Mouse Y");


		// Adjust the look angle by an amount proportional to the turn speed and horizontal input.
		m_Tilt_X_Angle += y * m_TurnSpeed;

		m_Tilt_X_Angle = Mathf.Clamp(m_Tilt_X_Angle, -m_TiltMin, m_TiltMax);

		m_TransformTargetRot = _inverse ? Quaternion.Euler(m_Tilt_X_Angle, 0f, 0f) : Quaternion.Euler(-m_Tilt_X_Angle, 0f, 0f);

		transform.localRotation = m_TransformTargetRot;
	}

	public float GetLocalX_Rot()
	{
		return transform.localRotation.eulerAngles.x;
	}
}
