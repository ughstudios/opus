using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventsHandler : MonoBehaviour
{

	[SerializeField] float _jumpHeight = 10.0f; //sets the height of the jump
	[SerializeField] GameObject _throPos = null;
	[SerializeField] GameObject _flamePos = null;
	
	//this will allow the player to move on the Y-axis for jumping
	public void Jump()
	{
		GetComponentInParent<PlayerController>().SetCanJumpF();

		
		GetComponentInParent<Rigidbody>().velocity = new Vector3(GetComponentInParent<Rigidbody>().velocity.x, 
			1 * _jumpHeight, 
			GetComponentInParent<Rigidbody>().velocity.z);
		
		//GetComponentInParent<Rigidbody>().AddForce(Vector3.up * _jumpHeight, ForceMode.Impulse);
		GetComponent<Animator>().SetBool("isJumping", false);
		
		GetComponentInParent<PlayerController>().ResetJump();
	}

	public void Throw()
	{
		GetComponent<Animator>().SetBool("isThrowing", false);
		GetComponentInParent<PlayerController>().ResetMovementSpeed();
		_throPos.GetComponent<InstantiateSpell>().ThrowSpell();
	}

	public void Flame()
	{
		_flamePos.GetComponent<InstantiateFire>().FireAttack();
	}

	public void StopFireAttack()
	{
		_flamePos.GetComponent<InstantiateFire>().StopFireAttack();
	}
	//TODO - Setup Cool down and Launch snipper prefab particle

	public void StartSniperAttack()
	{
		_throPos.GetComponent<InstantiateSpell>().ThrowSnipperPorjectile();
	}

	public void StopSnipperAttack()
	{
		GetComponentInParent<PlayerController>().SetIsSnippingToFalse();
	}

	public void ResetMoveSpeed()
	{
		GetComponentInParent<PlayerController>().ResetMovementSpeed();
	}

	public void ZeroMoveSpeed()
	{
		GetComponentInParent<PlayerController>().MovementSpeedZero();
	}
}
