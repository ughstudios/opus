using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;

public class SpellManager : MonoBehaviour
{

	public void ThrowSpell(string playerWhoSpawnedUs)
	{
		TravelingSpell spell = NetworkManager.Instance.InstantiateThrowObj(0, transform.position, transform.rotation) as TravelingSpell;
		Debug.Log("player Who Spoawned us::SpellManager: " + playerWhoSpawnedUs);
		spell.playerWhoSpawnedUs = playerWhoSpawnedUs;
	}
	

	public void ThrowSnipperPorjectile(string playerWhoSpawnedUs)
	{
		TravelingSpell spell = NetworkManager.Instance.InstantiateThrowObj(1, transform.position, transform.rotation) as TravelingSpell;
		spell.playerWhoSpawnedUs = playerWhoSpawnedUs;
	}

}
