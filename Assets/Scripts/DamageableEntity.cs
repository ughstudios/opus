using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableEntity : MonoBehaviour
{
    public int health = 100;

    public int MaxEverything = 100;
    public int damage = 0;
    public float damageForce = 0f;
    public float damageRecoilForce = 0f;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = MaxEverything;
    }

    protected virtual void FixedUpdate()
    {
        if (transform.position.y < -1000)
        {
            OnDeath();
        }
    }

    public virtual int TakeDamage(DamageableEntity source, int damage)
    {
        if (damage < 1)
            return 0;
        int damageDealt = Mathf.Min(damage, health);
        health -= damageDealt;
        if (health <= 0)
        {
            OnDeath();
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (damageDealt > 0 && rb != null)
        {
            rb.AddForce((transform.position - source.transform.position).normalized *
                    source.damageForce * rb.mass, ForceMode.Impulse);
        }
        Rigidbody sourceRb = source.GetComponent<Rigidbody>();
        if (damageDealt > 0 && sourceRb != null)
        {
            sourceRb.AddForce((source.transform.position - transform.position).normalized *
                    source.damageRecoilForce * sourceRb.mass, ForceMode.Impulse);
        }

        return damageDealt;
    }

    protected virtual void OnDeath()
    {
        Destroy(gameObject, 2f);
        Destroy(this);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        DamageableEntity de = null;

        if (damage > 0 && 
                (de = collision.gameObject.GetComponent<DamageableEntity>()) != null)
        {
            de.TakeDamage(this, damage);
        }
    }
}
