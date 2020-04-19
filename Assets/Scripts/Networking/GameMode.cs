using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;

public class GameMode : GameModeBehavior
{

    // Have lobby send list of ip's to the server to verify who should be on the server during any given match. 
    Vector3 spawnPoint;
    public float initialMatchTimer = 300;
    private float matchTimer; // Match countdown timer in seconds. 300 seconds = 5 minutes. 

    private bool serverHasBeenReset = false;

    protected override void NetworkStart()
    {
        base.NetworkStart();

    }

    void Update()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.Networker != null)
        {
            if (NetworkManager.Instance.Networker.Players.Count > 1)
            {
                matchTimer -= Time.deltaTime;
                if (matchTimer < 0)
                {
                    if (!serverHasBeenReset)
                    {
                        ResetServer();
                        serverHasBeenReset = true;
                        matchTimer = initialMatchTimer;
                    }
                }
            }
        }

    }

    void ResetServer()
    {
        MainThreadManager.Run(() =>
        {
            lock (NetworkManager.Instance.Networker.Players)
            {
                foreach (var player in NetworkManager.Instance.Networker.Players)
                {
                    ((IServer)NetworkManager.Instance.Networker).Disconnect(player, true);
                    matchTimer = initialMatchTimer;
                    serverHasBeenReset = true;
                }
                NetworkManager.Instance.UpdateMasterServerListing(NetworkManager.Instance.Networker, "Opus", "BattleRoyale", "Solo");
            }
        });
    }

    void Start()
    {
        if (!networkObject.IsServer)
        {
            return;
        }

        matchTimer = initialMatchTimer;

        Debug.Log("Game mode is being initialized!");

        NetworkManager.Instance.Networker.playerAccepted += Networker_playerAccepted;
        NetworkManager.Instance.Networker.playerDisconnected += Networker_playerDisconnected;
        NetworkManager.Instance.Networker.playerRejected += Networker_playerRejected;
        NetworkManager.Instance.Networker.playerTimeout += Networker_playerTimeout;
        NetworkManager.Instance.Networker.playerConnected += Networker_playerConnected;

    }




    private void Networker_playerConnected(NetworkingPlayer player, NetWorker sender)
    {

    }

    private void Networker_playerTimeout(NetworkingPlayer player, NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            NetworkManager.Instance.UpdateMasterServerListing(NetworkManager.Instance.Networker, "Opus", "BattleRoyale", "Solo");
        });
    }

    private void Networker_playerRejected(NetworkingPlayer player, NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            Debug.LogError("Player rejected!");
        });

    }

    private void Networker_playerAccepted(NetworkingPlayer player, NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {

            GameObject go = GameObject.FindWithTag("Spawn Point");
            if (go)
            {
                spawnPoint = go.transform.position;
            }

            //PlayerController playerController = NetworkManager.Instance.InstantiatePlayer(position: spawnPoint) as PlayerController;
            NewCharacterController playerController = NetworkManager.Instance.InstantiatePlayer(position: spawnPoint) as NewCharacterController;

            playerController.networkObject.position = spawnPoint;
            playerController.networkObject.AssignOwnership(player);

            Debug.Log("Player Count: " + NetworkManager.Instance.Networker.Players.Count);
            NetworkManager.Instance.UpdateMasterServerListing(NetworkManager.Instance.Networker, "Opus", "BattleRoyale", "Solo");

            Debug.Log("Player Connected: " + player.Ip);
        });
    }

    private void Networker_playerDisconnected(NetworkingPlayer player, NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            Debug.Log("Player Count: " + NetworkManager.Instance.Networker.Players.Count);
            NetworkManager.Instance.UpdateMasterServerListing(NetworkManager.Instance.Networker, "Opus", "BattleRoyale", "Solo");

            Debug.Log("Disconnect");
            //Loop through all players and find the player who disconnected, store all it's networkobjects to a list
            List<NetworkObject> toDelete = new List<NetworkObject>();
            foreach (var no in sender.NetworkObjectList)
            {
                if (no.Owner == player)
                {
                    //Found him
                    toDelete.Add(no);
                    Debug.Log("owner found");
                }
                //TODO - Correct issues with disconnecting
            }

            //Remove the actual network object outside of the foreach loop, as we would modify the collection at runtime elsewise. (could also use a return, too late)
            if (toDelete.Count > 0)
            {
                for (int i = toDelete.Count - 1; i >= 0; i--)
                {
                    sender.NetworkObjectList.Remove(toDelete[i]);
                    toDelete[i].Destroy();
                }
            }
        });
    }

    private void OnApplicationQuit()
    {
        NetWorker.EndSession();
    }
}
