using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class InstantiateSpell : InstatiateSpellPosBehavior
{
	[SerializeField] GameObject _testSpell;

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

    // Start is called before the first frame update
    public void ThrowSpell()
    {
		if (networkObject != null)
		{
			Instantiate(_testSpell, networkObject.position, networkObject.rotation);
		}
		else
			Instantiate(_testSpell, transform.position, transform.rotation);
	}

	public void SetSpell(GameObject spellObj)
	{
		_testSpell = spellObj;
	}
}
