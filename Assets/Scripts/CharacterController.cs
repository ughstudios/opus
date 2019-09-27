using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterController : MonoBehaviour
{
    public float movementSpeed = 5f;
	public float jumpSpeed = 5f;
    private Animator animator;
    private Rigidbody rigidbody;
	private bool isGrounded = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
    }

	void OnCollisionStay(Collision collisionInfo)
	{
		isGrounded = true;
	}
	
    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(horizontal * movementSpeed, 0, 0);
        rigidbody.AddForce(movement);
        
		if (horizontal != 0)
		{
			animator.SetBool("Walking?", true);
		}
		else
		{
			animator.SetBool("Walking?", false);
		}
		
		Vector3 ourLocalScale = transform.localScale;
		
		if (horizontal > 0)
		{
			ourLocalScale.x = 1;
			transform.localScale = ourLocalScale;
		}
		if (horizontal < 0)
		{
			ourLocalScale.x = -1;
			transform.localScale = ourLocalScale;
		}
		
		bool bJumping = Input.GetButtonDown("Jump");
		Debug.Log("isGrounded:" + isGrounded);
		
		if (bJumping && isGrounded)
		{
			rigidbody.AddForce(new Vector3(0, jumpSpeed, 0), ForceMode.Impulse);
			isGrounded = false;
		}
        		
		if (horizontal == 0 && !bJumping && !IsFalling() && isGrounded)
		{
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
		}
		
    }
	
	
	private bool IsFalling()
	{
		return rigidbody.velocity.y < 0;
	}
}
