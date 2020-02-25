using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumping : MainCharGlobal
{
	//set the max height for the player's jump
	[Range(0, 8)]
	[SerializeField] float _jumpHeight = 10.0f;

    // Update is called once per frame
    void Update()
    {
		if (Input.GetButtonDown(CharacterButtonsConstants.JUMP) && _groundDetection.OnGround)
		{
			//Jump();
		}
    }

	//this will allow the player to move on the Y-axis for jumping
	public void Jump()
	{
		_rb.velocity = Vector3.up * _jumpHeight;
	}
}
