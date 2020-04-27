using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;

public class SpellManager : MonoBehaviour
{
	public GameObject _lighteningPS;
	public GameObject _spell;

	public void ThrowSpell(string playerWhoSpawnedUs, uint netId)
	{
		TravelingSpell spell = NetworkManager.Instance.InstantiateThrowObj(0, transform.position, GameObject.FindGameObjectWithTag("MainCamera").transform.rotation) as TravelingSpell;
		Debug.Log("player Who Spoawned us::SpellManager: " + playerWhoSpawnedUs);
		spell.playerWhoSpawnedUs = playerWhoSpawnedUs;
		spell._netId = netId;
	}

	public void ThrowSpell()
	{
		Instantiate(_spell, transform.position, GameObject.FindGameObjectWithTag("MainCamera").transform.rotation);
	}


	public void ThrowSnipperPorjectile(string playerWhoSpawnedUs, uint netId)
	{
		TravelingSpell spell = NetworkManager.Instance.InstantiateThrowObj(1, transform.position, GameObject.FindGameObjectWithTag("MainCamera").transform.rotation) as TravelingSpell;
		spell.playerWhoSpawnedUs = playerWhoSpawnedUs;
		spell._netId = netId;
	}

	public void ThrowSnipperPorjectile()
	{
		Instantiate(_lighteningPS, transform.position, GameObject.FindGameObjectWithTag("MainCamera").transform.rotation);
	}

}
