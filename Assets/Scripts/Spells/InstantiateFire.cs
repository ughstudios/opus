using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;


public class InstantiateFire : InstantiateFirePosBehavior
{
	[SerializeField] GameObject _fireAttackPS = null;
	[SerializeField] GameObject _fireAttackHolder = null;
	

	void Update()
    {
		if (networkObject != null)
		{
			networkObject.position = transform.position;
			networkObject.rotation = transform.rotation;

			
			/*
			if (networkObject.IsOwner)
			{
				networkObject.position = transform.position;
				networkObject.rotation = transform.rotation;
			}
			else {
				transform.position = networkObject.position;
				transform.rotation = networkObject.rotation;
			}
			*/
			
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
