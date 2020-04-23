using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameMode : GameModeBehavior, IUserAuthenticator
{
    private Server.AuthStatus status = Server.AuthStatus.Available;
    private List<uint> allowedIds;

    // Have lobby send list of ip's to the server to verify who should be on the server during any given match. 
    public float initialMatchTimer = 300; // Match countdown timer in seconds. 300 seconds = 5 minutes. 

    private bool serverHasBeenReset = false;


    protected override void NetworkStart()
    {
        base.NetworkStart();

        networkObject.matchTimer = initialMatchTimer;


    }

    void Update()
    {
        if (!networkObject.IsOwner)
            return;
        
        if (NetworkManager.Instance != null && NetworkManager.Instance.Networker != null)
        {
            lock (NetworkManager.Instance.Networker.Players)
            {
                networkObject.playerCount = NetworkManager.Instance.Networker.Players.Count - 1;

                if (NetworkManager.Instance.Networker.Players.Count >= 2)
                {
                    networkObject.matchTimer -= Time.deltaTime;
                    if (networkObject.matchTimer < 0)
                    {
                        ResetServer();
                        serverHasBeenReset = true;
                        networkObject.matchTimer = initialMatchTimer;
                    }
                }
                else
                {
                    serverHasBeenReset = false;
                }
            }
        }
        

    }

    void ResetServer()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.Networker != null)
        {
            lock (NetworkManager.Instance.Networker.Players)
            {
                foreach (var player in NetworkManager.Instance.Networker.Players)
                {
                    if (!player.IsHost)
                    {
                        ((IServer)NetworkManager.Instance.Networker).Disconnect(player, true);

                        NewCharacterController[] characters = FindObjectsOfType<NewCharacterController>();
                        foreach (var character in characters)
                        {
                            character.networkObject.Destroy();
                            Destroy(character);
                        }
                    }

                }
                serverHasBeenReset = true;
                status = Server.AuthStatus.Available;
                NetworkManager.Instance.UpdateMasterServerListing(NetworkManager.Instance.Networker, "Opus", "BattleRoyale", "Solo");
            }
        }

    }

    void Start()
    {
        if (!networkObject.IsServer)
        {
            return;
        }

        //networkObject.matchTimer = initialMatchTimer;

        Debug.Log("Game mode is being initialized!");

        NetworkManager.Instance.Networker.playerAccepted += Networker_playerAccepted;
        NetworkManager.Instance.Networker.playerDisconnected += Networker_playerDisconnected;
        NetworkManager.Instance.Networker.playerRejected += Networker_playerRejected;
        NetworkManager.Instance.Networker.playerTimeout += Networker_playerTimeout;
        NetworkManager.Instance.Networker.playerConnected += Networker_playerConnected;



        //NetworkManager.Instance.Networker.SetUserAuthenticator(this);

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
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Spawn Point");

            Vector3 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;

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


    public void IssueChallenge(NetWorker networker, NetworkingPlayer player, System.Action<NetworkingPlayer, BMSByte> issueChallengeAction, System.Action<NetworkingPlayer> skipAuthAction)
    {
        issueChallengeAction(player, ObjectMapper.BMSByte(status));
    }

    public void AcceptChallenge(NetWorker networker, BMSByte challenge, System.Action<BMSByte> authServerAction, System.Action rejectServerAction)
    {
        //throw new NotImplementedException();
    }

    public void VerifyResponse(NetWorker networker, NetworkingPlayer player, BMSByte response, System.Action<NetworkingPlayer> authUserAction, System.Action<NetworkingPlayer> rejectUserAction)
    {
        uint id;
        switch (status)
        {
            case Server.AuthStatus.Available:
                id = response.GetBasicType<uint>();

                BinaryFormatter binFor = new BinaryFormatter();
                MemoryStream memStream = new MemoryStream();

                byte[] bytes = response.GetByteArray(sizeof(uint));
                memStream.Write(bytes, 0, bytes.Length);
                memStream.Position = 0;

                List<uint> ids = binFor.Deserialize(memStream) as List<uint>;

                if (ids.Contains(id))
                {
                    ids.Remove(id);
                    authUserAction(player);
                    status = Server.AuthStatus.Checking;
                    allowedIds = ids;
                }
                else
                {
                    rejectUserAction(player);
                }

                //uint[] ids = ObjectMapper.Instance.Map<uint[]>(response);
                break;
            case Server.AuthStatus.Checking:
                id = response.GetBasicType<uint>();
                if (allowedIds.Contains(id))
                {
                    authUserAction(player);
                    allowedIds.Remove(id);
                }
                else
                {
                    rejectUserAction(player);
                }
                return;
            case Server.AuthStatus.Closed:
                rejectUserAction(player);
                return;
        }
    }
}
