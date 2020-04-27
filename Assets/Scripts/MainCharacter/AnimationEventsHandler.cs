using Steamworks;
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

		if (GetComponentInParent<NewCharacterController>().networkObject != null)
		{
			if (GetComponentInParent<NewCharacterController>().networkObject.IsOwner)
			{
				_throPos.GetComponent<SpellManager>().ThrowSpell(GetComponentInParent<NewCharacterController>().playerName, GetComponentInParent<NewCharacterController>()._idNum);
			}
		}
		
		if(GetComponentInParent<NewCharacterController>().networkObject == null) 
		{
			_throPos.GetComponent<SpellManager>().ThrowSpell();//this is for testing outside of network play
		}
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
		if (GetComponentInParent<NewCharacterController>().networkObject.IsOwner)
		{
			Debug.Log("SteamName" + SteamClient.Name);
			_throPos.GetComponent<SpellManager>().ThrowSpell(GetComponentInParent<NewCharacterController>().playerName, GetComponentInParent<NewCharacterController>()._idNum);
		}
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
		if (GetComponentInParent<NewCharacterController>().networkObject != null)
		{
			if (GetComponentInParent<NewCharacterController>().networkObject.IsOwner)
			{
				_throPos.GetComponent<SpellManager>().ThrowSnipperPorjectile(GetComponentInParent<NewCharacterController>().playerName, GetComponentInParent<NewCharacterController>()._idNum);
			}
		}

		if (GetComponentInParent<NewCharacterController>().networkObject == null)
		{
			_throPos.GetComponent<SpellManager>().ThrowSnipperPorjectile();//this is for testing outside of network play
		}
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
