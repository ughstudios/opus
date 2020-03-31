using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;

public class InstantiateGameMode : MonoBehaviour
{
    void Start()
    {
		/*
		if (NetworkManager.Instance != null)
		{
			if (NetworkManager.Instance.IsServer)
				NetworkManager.Instance.InstantiateGameMode();
		}
		*/
	
		

		if (NetworkManager.Instance != null)
		{
			if (!NetworkManager.Instance.IsServer)
				NetworkManager.Instance.InstantiatePlayer();
		}
		
	}
}
