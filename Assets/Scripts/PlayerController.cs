using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MobController
{
    public int food = 100;

    public int water = 100;

    

    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.tag == "Water Hex")
        {
            if (Input.GetButtonDown("Gather"))
            {
                water = 100;
                SurvivalTimer survivalTimer = GetComponent<SurvivalTimer>();
                survivalTimer.update_water_bar();
            }
        }
    }

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
        SceneManager.LoadScene("GameOver");
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        GroundCheck();
        if (collision.collider == groundCollider)
            base.OnCollisionEnter(collision);
    }
}
