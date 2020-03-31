using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity.Lobby;
using BeardedManStudios.Forge.Networking;

public class Lobby_PlayersCounter : MonoBehaviour
{
	[SerializeField] LobbyManager _lobbyManager;
	NetWorker _server = new UDPServer(3);

	void Update()
	{
		Debug.Log("Player in the scene: " + _lobbyManager.PlayerCount());

		//if the max amount of players per match is reached we stop all incomming connections
		//using 2 for test purposes
		if (_lobbyManager.PlayerCount() > 2)
		{
			GameObject _server = GameObject.FindGameObjectWithTag("HostServer").gameObject;
			((IServer)_server.GetComponent<Server>().server).StopAcceptingConnections();
			_lobbyManager.StartGame(2);
		}
			
	}
}
