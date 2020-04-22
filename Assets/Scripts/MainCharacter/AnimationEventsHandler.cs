using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventsHandler : MonoBehaviour
{

	[SerializeField] float _jumpHeight = 10.0f; //sets the height of the jump
	[SerializeField] GameObject _throPos = null;
	[SerializeField] GameObject _flamePos = null;




	#region No Rigidbody calls 
	
	//the new player controller does not have a rigid body

	public void JumpWithoutRb()
	{
		GetComponentInParent<NewCharacterController>().Jump();
	}

	public void ThrowWithoutRb()
	{
		GetComponent<Animator>().SetBool("isThrowing", false);
		GetComponentInParent<NewCharacterController>().ResetMovementSpeed();
		_throPos.GetComponent<SpellManager>().ThrowSpell();
	}

	public void StopSnipperAttackNoRb()
	{
		GetComponentInParent<NewCharacterController>().SetIsSnippingToFalse();
	}

	public void ZeroMoveSpeedNoRb()
	{
		GetComponentInParent<NewCharacterController>().MovementSpeedZero();
	}

	public void ResetMoveSpeedNoRb()
	{
		GetComponentInParent<NewCharacterController>().ResetMovementSpeed();
	}

	public void SetDecentNoRb()
	{
		GetComponentInParent<NewCharacterController>().SetDecentToTrue();
	}

	#endregion

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
		_throPos.GetComponent<SpellManager>().ThrowSpell();
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
		_throPos.GetComponent<SpellManager>().ThrowSnipperPorjectile();
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
