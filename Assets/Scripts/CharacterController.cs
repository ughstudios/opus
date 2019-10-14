using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public float movementSpeed = 15f;
    public float moveForceMult = 5f;
    public float jumpSpeed = 5f;
    private Animator animator;
    private Rigidbody rigidbody;
    private bool isGrounded = false;
    private Vector3 targetRot;
    public Vector3 rightRot = Vector3.zero;
    public Vector3 leftRot = new Vector3(0f, 180f, 0f);
    public float rotSpeed = 4f;

    void Start()
    {
        //        animator = GetComponent<Animator>();
        animator = GetComponentInChildren<Animator>();
        rigidbody = GetComponent<Rigidbody>();

        targetRot = rightRot;
    }

    void OnCollisionStay(Collision collisionInfo)
    {
        isGrounded = true;
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(horizontal, 0, vertical);
        moveInput.Normalize();

        Vector3 movement = moveInput * movementSpeed;
        Vector3 horizVel = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);

//        Debug.Log("horizVel: " + horizVel + " movement: " + movement);

        if (horizVel.sqrMagnitude < movement.sqrMagnitude)
        {
            rigidbody.AddForce(movement * moveForceMult, ForceMode.Impulse);
        }

        if (moveInput.sqrMagnitude > Mathf.Epsilon)
        {
            animator.SetBool("Walking?", true);
        }
        else
        {
            animator.SetBool("Walking?", false);
        }

        if (horizontal > 0)
        {
            targetRot = rightRot;
        }

        if (horizontal < 0)
        {
            targetRot = leftRot;
        }
        animator.transform.localEulerAngles = Vector3.Lerp(animator.transform.localEulerAngles,
                targetRot, rotSpeed * Time.fixedDeltaTime);
        /*
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
        */
        bool bJumping = Input.GetButtonDown("Jump");
//        Debug.Log("isGrounded:" + isGrounded);

        if (bJumping && isGrounded)
        {
            rigidbody.AddForce(new Vector3(0, jumpSpeed, 0), ForceMode.Impulse);
            isGrounded = false;
        }

        if (moveInput.sqrMagnitude < Mathf.Epsilon && !bJumping && !IsFalling() && isGrounded)
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
