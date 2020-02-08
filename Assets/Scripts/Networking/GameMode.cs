using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;


public class GameMode : GameModeBehavior
{

    Vector3 spawnPoint;

    protected override void NetworkStart()
    {
        base.NetworkStart();

    }

    void Start()
    {

        if (!networkObject.IsServer)
        {
            return;
        }

        Debug.Log("Game mode is being initialized!");


        NetworkManager.Instance.Networker.playerAccepted += Networker_playerAccepted;
        NetworkManager.Instance.Networker.playerDisconnected += Networker_playerDisconnected;
        NetworkManager.Instance.Networker.playerRejected += Networker_playerRejected;
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
            spawnPoint = go.transform.position;

            PlayerController playerController = NetworkManager.Instance.InstantiatePlayer(position: spawnPoint) as PlayerController;
            //playerController.networkObject.position = spawnPoint;
            playerController.networkObject.AssignOwnership(player);
            
            Debug.Log("Player Connected: " + player.Ip);
        });
    }

    private void Networker_playerDisconnected(NetworkingPlayer player, NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
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
