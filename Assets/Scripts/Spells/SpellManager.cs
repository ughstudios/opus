using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;

public class SpellManager : MonoBehaviour
{
	
	public void ThrowSpell()
    {
		NetworkManager.Instance.InstantiateThrowObj(0, transform.position, transform.rotation);
	}

	public void ThrowSnipperPorjectile()
	{
		NetworkManager.Instance.InstantiateThrowObj(1, transform.position, transform.rotation);
	}

}
