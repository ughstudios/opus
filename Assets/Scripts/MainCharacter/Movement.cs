using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MainCharGlobal
{
	[Range(0, 500)]
	[SerializeField] float _movementSpeed = 12.0f;  //this is for determining the speed the player will move at

	bool _isMoving = false;//if a movement button is pressed this will be true

	Vector3 currentEulerAngles = new Vector3();

	void Update()
    {
		if (Input.GetAxis(CharacterButtonsConstants.HORIZONTAL) > 0.75f || Input.GetAxis(CharacterButtonsConstants.HORIZONTAL) < -0.75f ||
			Input.GetAxis(CharacterButtonsConstants.VERTICLE) > 0.75f || Input.GetAxis(CharacterButtonsConstants.VERTICLE) < -0.75f)
		{
			_isMoving = true;
		}
		else { _isMoving = false; }


		//modifying the Vector3, based on input multiplied by speed and time
		
		currentEulerAngles += new Vector3(0, Input.GetAxis(CharacterButtonsConstants.ROTATE_H), 0) * Time.deltaTime * 50f;

		//apply the change to the gameObject
		//transform.eulerAngles = currentEulerAngles;

		transform.rotation = Quaternion.Euler(currentEulerAngles);

	}

	void FixedUpdate()
	{
		if (_isMoving)
		{
			MovePlayer();
		}
	}

	//Velocity for movement in the x and z pos applied here and rotation for the direction the player is facing
	void MovePlayer()
	{
		
		Vector3 normalizeInput = new Vector3(Input.GetAxis(CharacterButtonsConstants.ROTATE_H), 0f, 
												Input.GetAxis(CharacterButtonsConstants.VERTICLE));

		Vector3 normalized = Vector3.Normalize(normalizeInput);
		
		if (normalizeInput.x > 0 &&
			normalizeInput.z > 0 )
		{
			_rb.velocity = new Vector3(1 * _movementSpeed * Time.deltaTime,
									_rb.velocity.y, 1 * _movementSpeed * Time.deltaTime);
		}
		else if (Input.GetAxis(CharacterButtonsConstants.VERTICLE) > 0)
		{
			_rb.velocity = new Vector3(_rb.velocity.x,
									_rb.velocity.y, 1 * _movementSpeed * Time.deltaTime);
		}


		//transform.LookAt(transform.position + new Vector3(normalizeInput.x,0, normalizeInput.z));
	}
}
