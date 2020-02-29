using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventsHandler : MonoBehaviour
{

	[SerializeField] float _jumpHeight = 10.0f; //sets the height of the jump
	[SerializeField] GameObject _throPos = null;

	//this will allow the player to move on the Y-axis for jumping
	public void Jump()
	{
		GetComponentInParent<Rigidbody>().velocity = Vector3.up * _jumpHeight;

		GetComponent<Animator>().SetBool("isJumping", false);
		
		GetComponentInParent<PlayerController>().ResetJump();
	}

	public void Throw()
	{
		_throPos.GetComponent<InstantiateSpell>().ThrowSpell();
		GetComponent<Animator>().SetBool("isThrowing", false);
		GetComponentInParent<PlayerController>().ResetMovementSpeed();
	}
}
