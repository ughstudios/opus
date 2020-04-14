using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class InstantiateSpell : InstatiateSpellPosBehavior
{
	[SerializeField] GameObject _spell,
								_snipperProjectile;
	[SerializeField] Transform _xRotTransform = null;

	void Update()
	{
		if(_xRotTransform != null)
			transform.localRotation = Quaternion.Euler(_xRotTransform.transform.rotation.eulerAngles.x,0,0);
	
		if (networkObject != null)
		{
			if (!networkObject.IsOwner)
			{
				transform.position = networkObject.position;
				transform.rotation = networkObject.rotation;
				transform.localRotation = networkObject.localRotation;
			}

			networkObject.position = transform.position;
			networkObject.rotation = transform.rotation;
			networkObject.localRotation = transform.localRotation;
		}
	}

    public void ThrowSpell()
    {
		if (networkObject != null)
		{
			Instantiate(_spell, networkObject.position, networkObject.rotation);
		}
		else
			Instantiate(_spell, transform.position, transform.rotation);
	}

	public void ThrowSnipperPorjectile()
	{
		if (networkObject != null)
		{
			Instantiate(_snipperProjectile, networkObject.position, transform.rotation);
		}
		else
			Instantiate(_snipperProjectile, transform.position, transform.rotation);
	}

	public void SetSpell(GameObject spellObj)
	{
		_spell = spellObj;
	}
}
