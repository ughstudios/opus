using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerCamera : MonoBehaviour
{
	private float m_Tilt_X_Angle;                    // The rig's y axis rotation.
	[Range(0f, 10f)] [SerializeField] private float m_TurnSpeed = 1.5f;   // How fast the rig will rotate from user input.
	private Quaternion m_TransformTargetRot;
	[SerializeField] bool _inverse = false;
	[SerializeField] float	m_TiltMin = 45.0f,
							m_TiltMax = 45.0f;


	void Update()
    {
		HandleXRotation();
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
}
