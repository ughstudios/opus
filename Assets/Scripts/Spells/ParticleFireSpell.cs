using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using Steamworks;

public class ParticleFireSpell : MonoBehaviour
{
	[SerializeField] int _damage = 1;

	void OnParticleCollision(GameObject other)
	{
		if (other.tag == "Player")
		{
			DamageableEntity de = other.GetComponent<DamageableEntity>();
			
			de.TakeDamage(SteamClient.Name, _damage);
		}
	}

	public void DestroyThisParticleObj()
	{
		Destroy(gameObject);
	}
}
