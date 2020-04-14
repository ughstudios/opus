using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;


public class InstantiateFire : InstantiateFirePosBehavior
{
	[SerializeField] GameObject _fireAttackPS = null;
	[SerializeField] GameObject _fireAttackHolder = null;
	

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

	public void FireAttack()
	{
		_fireAttackHolder.SetActive(true);
		/*
		if (networkObject != null)
		{
			Instantiate(_fireAttackPS, networkObject.position, networkObject.rotation);
		}
		else
			Instantiate(_fireAttackPS, transform.position, transform.rotation);
		*/


		/*
		GameObject f = _fireAttackPS;
		f.name = "flamez";

		Instantiate(f, transform);
		*/
	}

	public void StopFireAttack()
	{
		_fireAttackHolder.SetActive(false);
	}

}
