using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MobController
{
    public int food = 100;

    public int water = 100;
<<<<<<< Updated upstream

    private bool canPlant = true;
=======
    
>>>>>>> Stashed changes

    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.tag == "Water Hex")
        {
            if (Input.GetButtonDown("Gather"))
            {
                water = 100;
                SurvivalTimer survivalTimer = GetComponent<SurvivalTimer>();
                survivalTimer.reset_water_bar_to_full();
            }
        }
        canPlant = false;
    }

    private void OnTriggerExit(Collider other)
    {
        canPlant = true;
    }

    protected override void FixedUpdate()
    {
        moveInput.Set(Input.GetAxisRaw("Horizontal"),
                Input.GetButton("Jump") ? 1f : 0f,
                Input.GetAxisRaw("Vertical"));

        base.FixedUpdate();

        if (canPlant && isGrounded && groundCollider != null &&
                groundCollider.GetComponent<DamageableEntity>() == null 
                && Input.GetButton("Gather"))
        {
            Inventory inv = GetComponent<Inventory>();
            RaycastHit hit;
            Item toPlant = inv.GetRandomPlantable();
            if (toPlant != null && Physics.Raycast(transform.position,
                    Vector3.down, out hit, 5f))
            {
                Harvestable plant = Instantiate<Harvestable>(toPlant.plantPrefab,
                        hit.point, Quaternion.Euler(0f, Random.value * 360f, 0f));
                plant.canHarvest = false;
                plant.ForceUpdate();
                inv.RemoveCountOfItemFromInventory(toPlant, 1);
            }
        }
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

    private bool IsUnder(Collider col)
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + groundCheckOffset, groundCheckRadius,
                Vector3.down, groundCheckDist);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == col)
                return true;
        }
        return false;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        GroundCheck();
        if (IsUnder(collision.collider))
            base.OnCollisionEnter(collision);
    }
}
