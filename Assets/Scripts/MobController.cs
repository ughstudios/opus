using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MobController : DamageableEntity
{
    public float movementSpeed = 5f;
    public float moveForceMult = 1f;
    public float jumpSpeed = 5f;
    protected Animator animator;
    protected Rigidbody rb;
    protected bool isGrounded = false;
    protected Vector3 targetRot;
    public Vector3 rightRot = Vector3.zero;
    public Vector3 leftRot = new Vector3(0f, 180f, 0f);
    public float rotSpeed = 10f;

    protected Vector3 moveInput = Vector3.zero;

    protected override void Start()
    {
        base.Start();

        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        targetRot = rightRot;
    }

    protected virtual void OnCollisionStay(Collision collisionInfo)
    {
        isGrounded = true;
    }

    protected virtual void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 horizMoveInput = new Vector3(moveInput.x, 0, moveInput.z);
        if (horizMoveInput.sqrMagnitude > 1)
            horizMoveInput.Normalize();

        Vector3 movement = horizMoveInput * movementSpeed;
        Vector3 horizVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (horizVel.sqrMagnitude < movement.sqrMagnitude)
        {
            rb.AddForce(movement * moveForceMult, ForceMode.Impulse);
        }

        if (horizMoveInput.sqrMagnitude > Mathf.Epsilon)
        {
            animator.SetBool("Walking?", true);
        }
        else
        {
            animator.SetBool("Walking?", false);
        }

        if (horizMoveInput.x > Mathf.Epsilon)
        {
            targetRot = rightRot;
        }

        if (horizMoveInput.x < -Mathf.Epsilon)
        {
            targetRot = leftRot;
        }
        animator.transform.localEulerAngles = Vector3.Lerp(animator.transform.localEulerAngles,
                targetRot, rotSpeed * Time.fixedDeltaTime);
        bool bJumping = moveInput.y > Mathf.Epsilon;

        if (bJumping && isGrounded)
        {
            //rigidbody.AddForce(new Vector3(0, jumpSpeed, 0), ForceMode.Impulse);
            rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
            isGrounded = false;
        }

        if (horizMoveInput.sqrMagnitude < Mathf.Epsilon && !bJumping && !IsFalling() && isGrounded)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

    }


    protected virtual bool IsFalling()
    {
        return rb.velocity.y < 0;
    }
}
