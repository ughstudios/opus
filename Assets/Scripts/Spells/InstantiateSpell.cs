using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using UnityStandardAssets.CrossPlatformInput;

public class InstantiateSpell : InstatiateSpellPosBehavior
{
	[SerializeField] GameObject _spell,
								_snipperProjectile;
	[SerializeField] Transform _xRotTransform = null;

	//rotation has to be handled here for networking purposes
	private float m_Tilt_X_Angle;                    // The rig's y axis rotation.
	[Range(0f, 10f)] [SerializeField] private float m_TurnSpeed = 1.5f;   // How fast the rig will rotate from user input.
	private Quaternion m_TransformTargetRot;
	[SerializeField] bool _inverse = false;
	[SerializeField]
	float m_TiltMin = 45.0f,
							m_TiltMax = 45.0f;

	float y = 0.0f;

	void Update()
	{
		//var x = CrossPlatformInputManager.GetAxis("Mouse X");

		SendPosAndRot();

		//if(networkObject == null)
		//	HandleXRotation(Input.GetAxis("Mouse Y"));

		HandleXRotation(Input.GetAxis("Mouse Y"));

	}
	void SendPosAndRot()
	{
		if (networkObject == null)
			return;

		if (networkObject.IsOwner)
		{
			networkObject.position = transform.position;
			networkObject.rotation = transform.rotation;
			//networkObject.m_TransformTargetRot = m_TransformTargetRot;
			networkObject.localRotation = transform.rotation;			
		}
		else
		{
			transform.position = networkObject.position;
			transform.rotation = networkObject.rotation;
			//m_TransformTargetRot = networkObject.m_TransformTargetRot;
			transform.localRotation = networkObject.localRotation;

		}
		

	}

	void HandleXRotation()
	{
		
		// Read the user input, we only need the Y axis because the cam is following the player rot
		//var y = CrossPlatformInputManager.GetAxis("Mouse Y");
		

		// Adjust the look angle by an amount proportional to the turn speed and horizontal input.
		m_Tilt_X_Angle += y * m_TurnSpeed;

		m_Tilt_X_Angle = Mathf.Clamp(m_Tilt_X_Angle, -m_TiltMin, m_TiltMax);

		m_TransformTargetRot = _inverse ? Quaternion.Euler(m_Tilt_X_Angle, 0f, 0f) : Quaternion.Euler(-m_Tilt_X_Angle, 0f, 0f);

		transform.localRotation = m_TransformTargetRot;
	}

	void HandleXRotation(float y)
	{
		Debug.Log("Calling from Owner Netowrk Play");

		// Read the user input, we only need the Y axis because the cam is following the player rot
		//var y = CrossPlatformInputManager.GetAxis("Mouse Y");


		// Adjust the look angle by an amount proportional to the turn speed and horizontal input.
		m_Tilt_X_Angle += y * m_TurnSpeed;

		m_Tilt_X_Angle = Mathf.Clamp(m_Tilt_X_Angle, -m_TiltMin, m_TiltMax);

		m_TransformTargetRot = _inverse ? Quaternion.Euler(m_Tilt_X_Angle, 0f, 0f) : Quaternion.Euler(-m_Tilt_X_Angle, 0f, 0f);

		transform.localRotation = m_TransformTargetRot;
	}

	public void ThrowSpell()
    {
		//GameObject projectile = GameObject.Instantiate(currentWeapon.projectile, currentWeapon.worldModelMuzzlePoint.transform.position, currentWeapon.worldModelMuzzlePoint.transform.rotation);

		GameObject projectile = GameObject.Instantiate(_spell, transform.position, transform.rotation);

		/*
		if (networkObject != null)
		{
			if (networkObject.IsOwner)
			{
				Instantiate(_spell, networkObject.position, transform.rotation);
			}
			else {
				Instantiate(_spell, transform.position, transform.rotation);
			}
		}

		if (networkObject == null)
		{
			Instantiate(_spell, transform.position, transform.rotation);
		}
		*/
	}

	public void ThrowSnipperPorjectile()
	{
		/*
		if (networkObject != null)
		{
			Instantiate(_snipperProjectile, networkObject.position, transform.rotation);
		}
		else
		*/
		
		Instantiate(_snipperProjectile, transform.position, transform.rotation);
	}

	public void SetSpell(GameObject spellObj)
	{
		_spell = spellObj;
	}
}
