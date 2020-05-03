using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;

public class InstantiateGameMode : MonoBehaviour
{
    void Start()
    {
		if (NetworkManager.Instance != null)
		{
			if (NetworkManager.Instance.IsServer)
				NetworkManager.Instance.InstantiateGameMode();
		}
		

		// This is for testing animations, DON'T TOUCH!
		/*
		if (NetworkManager.Instance != null)
		{
			if (!NetworkManager.Instance.IsServer)
				NetworkManager.Instance.InstantiateAnimTestObj();
		}
		*/

		//For testing purposes
		/*
		if (NetworkManager.Instance != null)
		{
			if (!NetworkManager.Instance.IsServer)
				NetworkManager.Instance.InstantiatePlayer();
		}
		*/

	}
}
