using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableEntity : MonoBehaviour
{
    public int health;
    public int maxHealth = 100;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = maxHealth;
    }

    public virtual int TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
            OnDeath();
        return damage;
    }

    protected virtual void OnDeath()
    {
        Destroy(gameObject, 2f);
    }
}
