using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;

public class ParticleFireSpell : FlameAttacckBehavior
{
	[SerializeField] int _damage = 1;

	protected override void NetworkStart()
	{
		base.NetworkStart();
		networkObject.position = transform.position;
		networkObject.rotation = transform.rotation;

		networkObject.positionInterpolation.target = transform.position;
		networkObject.rotationInterpolation.target = transform.rotation;

		networkObject.SnapInterpolations();
	}

	void Update()
	{

		if (networkObject != null)
		{
			if (!networkObject.IsOwner)
			{
				transform.position = networkObject.position;
				transform.rotation = networkObject.rotation;
			}

			networkObject.position = transform.position;
			networkObject.rotation = transform.rotation;
		}
	}


	void OnParticleCollision(GameObject other)
	{
		if (other.tag == "Player")
		{
			DamageableEntity de = other.GetComponent<DamageableEntity>();

			de.TakeDamage(de, _damage);
		}
	}

	public void DestroyThisParticleObj()
	{
		Destroy(gameObject);
	}
}
