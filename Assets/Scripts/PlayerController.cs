using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MobController
{

    protected override void FixedUpdate()
    {
        moveInput.Set(Input.GetAxisRaw("Horizontal"),
                Input.GetButton("Jump") ? 1f : 0f,
                Input.GetAxisRaw("Vertical"));

        base.FixedUpdate();
    }

    public override int TakeDamage(DamageableEntity source, int damage)
    {
        if (!isGrounded || groundCollider.GetComponent<DamageableEntity>() != null)
            return 0;

        return base.TakeDamage(source, damage);
    }

    protected override void OnDeath()
    {
        //base.OnDeath();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        GroundCheck();
        if (collision.collider == groundCollider)
            base.OnCollisionEnter(collision);
    }
}
