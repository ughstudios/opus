using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GenericHostile : MobController
{
    public float daySearchRange = 20f;
    public float nightSearchRange = 50f;
    public float searchInterval = 5f;
    public float wanderTime = 5f;
    protected System.Type[] targets = {typeof(PlayerController)};
    protected GameObject target;
    protected GameObject[] potentialTargets = new GameObject[32];
    protected int numPotentialTargets;
    protected Vector3 wanderDir;
    protected float searchRange;

    protected override void Start()
    {
        base.Start();

        StartCoroutine(CheckForTargets());
        StartCoroutine(PickWanderDir());

        searchRange = Mathf.Max(daySearchRange, nightSearchRange);
    }

    protected virtual IEnumerator PickWanderDir()
    {
        while (true)
        {
            yield return new WaitForSeconds(searchInterval);

            wanderDir = Random.onUnitSphere;
            wanderDir.y = 0f;
            wanderDir.Normalize();
        }
    }

    protected virtual IEnumerator CheckForTargets()
    {
        while (true)
        {
            yield return new WaitForSeconds(searchInterval);

            if (DayNightCycle.cycle != null)
            {
                searchRange = DayNightCycle.cycle.IsDayTime() ?
                        daySearchRange : nightSearchRange;
            }
            Collider[] cols = Physics.OverlapSphere(transform.position, searchRange);
            target = null;
            numPotentialTargets = 0;

            foreach (System.Type t in targets)
            {
                foreach (Collider col in cols)
                {
                    if (col.GetComponent(t) != null)
                    {
                        potentialTargets[numPotentialTargets] = col.gameObject;
                        numPotentialTargets++;
                    }
                }
            }

            if (numPotentialTargets > 0)
            {
                float closestDist = Mathf.Infinity;
                float dist;
                foreach (GameObject obj in potentialTargets)
                {
                    if (obj == null)
                        continue;
                    dist = Vector3.Distance(transform.position, obj.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = obj;
                    }
                }
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (target != null)
        {
            moveInput = (target.transform.position - transform.position).normalized;
            moveInput.y = 0f;
        }
        else
            moveInput = wanderDir;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        DamageableEntity de = collision.gameObject.GetComponent<DamageableEntity>();

        if (damage > 0 & de != null)
        {
            foreach (System.Type t in targets)
            {
                if (de.GetComponent(t) != null)
                {
                    de.TakeDamage(this, damage);
                    break;
                }
            }
        }
    }
}
