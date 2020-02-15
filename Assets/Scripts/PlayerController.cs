using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MobController
{
    public int food = 100;

    public int water = 100;
    private bool canPlant = true;
    public Camera camera;

    protected override void NetworkStart()
    {
        base.NetworkStart();
        networkObject.position = transform.position;
        networkObject.rotation = transform.rotation;
        networkObject.positionInterpolation.target = transform.position;
        networkObject.rotationInterpolation.target = transform.rotation;

        networkObject.SnapInterpolations();

    }


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

    private void Update()
    {
        if (networkObject.IsOwner)
        {
            camera.enabled = true;
        }
    }

    protected override void FixedUpdate()
    {
        if (!networkObject.IsOwner)
        {
            transform.position = networkObject.position;
            transform.rotation = networkObject.rotation;

            camera.enabled = false;

            return;
        }

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


        networkObject.position = transform.position;
        networkObject.rotation = transform.rotation;
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
