using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationsManager : MonoBehaviour
{

	[SerializeField] Animator _playerAnim;
	[SerializeField] float _velocityTriiger = 0.75f;
	[SerializeField] GroundDetection _groundDetection;


	// Update is called once per frame
	void Update()
    {
		//this is to trigger the walking animation ---> moved to original character controller
		/*
		if (_groundDetection.OnGround)
		{
			if (_rb.velocity.x > _velocityTriiger || _rb.velocity.x < -_velocityTriiger ||
			_rb.velocity.z > _velocityTriiger || _rb.velocity.z < -_velocityTriiger)
				_playerAnim.SetInteger("runningVal", 1);
			else if (_rb.velocity.x < .25f || _rb.velocity.x > -.25f ||
			_rb.velocity.z < .25f || _rb.velocity.z > -.25f)
				_playerAnim.SetInteger("runningVal", 0);
		}
		*/

		//testing the launch of a jump
		if (Input.GetButtonDown(CharacterButtonsConstants.JUMP) && _groundDetection.OnGround)
		{
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			//GetComponent<Movement>().enabled = false;
			GetComponent<PlayerController>().enabled = false;
			_playerAnim.SetBool("isJumping", true);
		}

		//for landing purposes
		_playerAnim.SetBool("onGround", _groundDetection.OnGround);
	}
}
