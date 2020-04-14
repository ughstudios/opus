using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;

public class AnimTester : AnimTestObjBehavior
{
	[SerializeField] int fireInt = 0;
	[SerializeField] Animator _anim = null;
	[SerializeField] GameObject _camera = null;
	protected override void NetworkStart()
	{
		base.NetworkStart();

		networkObject.position = transform.position;
	}

	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
    {
		if (Input.GetKey(KeyCode.J))
		{
			fireInt = 1;
		}
		else {
			fireInt = 0;
		}

		_anim.SetInteger("fireInt", fireInt);

		if (networkObject != null)
		{
			if (networkObject.IsOwner)
			{
				networkObject.fireInt = _anim.GetInteger("fireInt");
			}

			if (!networkObject.IsOwner)
			{
				if (_anim.GetInteger("fireInt") != networkObject.fireInt)
					_anim.SetInteger("fireInt", networkObject.fireInt);
			}
		}
    }
}
