using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using Steamworks;

public class GameMode : GameModeBehavior, IUserAuthenticator
{
    private Server.AuthStatus status = Server.AuthStatus.Available;
    private List<uint> allowedIds;

    // Have lobby send list of ip's to the server to verify who should be on the server during any given match. 
    public float initialMatchTimer = 300; // Match countdown timer in seconds. 300 seconds = 5 minutes. 

    private bool serverHasBeenReset = false;
    private List<NetworkObject> toDelete = new List<NetworkObject>();

    protected override void NetworkStart()
    {
        base.NetworkStart();

    }

    public override void AllPlayersLeaveLobby(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            ClientConnect clientScript = FindObjectOfType<ClientConnect>();
            if (clientScript == null)
                return;

            Debug.Log("client lobby before  leaving: " + clientScript.ourLobbyId.Value);
            clientScript.GetLobby().Leave();
            clientScript.ourLobbyId = new SteamId();
            Debug.Log("client lobby after leaving: " + clientScript.ourLobbyId.Value);

        });
    }

    void Update()
    {
        if (networkObject == null)
        {
            return;
        }

        if (!networkObject.IsServer)
        {
            return;
        }

        //Debug.Log("Number of game modes" + FindObjectsOfType<GameMode>().Length);

        if (NetworkManager.Instance != null && NetworkManager.Instance.Networker != null)
        {
            networkObject.playerCount = NetworkManager.Instance.Networker.Players.Count;

            if (NetworkManager.Instance.Networker.Players.Count <= 1 && networkObject.matchTimer < initialMatchTimer - 1)
            {
                ResetServer();
            }

            if (NetworkManager.Instance != null && NetworkManager.Instance.Networker != null)
            {
                if (NetworkManager.Instance.Networker.Players.Count >= 2)
                {
                    networkObject.matchTimer -= Time.deltaTime;
                    if (networkObject.matchTimer < 0)
                    {
                        Debug.Log("Update::About to reset server");
                        ResetServer();

                    }
                }
            }
        }

    }

    void DeleteObjects()
    {
        Debug.Log("deleting terrain and network objects");

        //NetworkManager networkMgr = FindObjectOfType<NetworkManager>();
        //MainThreadManager mainThreadMgr = FindObjectOfType<MainThreadManager>();
        //if (networkMgr != null)// && mainThreadMgr != null)
        //{
            //Destroy(networkMgr.gameObject);
            //Destroy(mainThreadMgr.gameObject);
        //}
        
        foreach (var character in FindObjectsOfType<NewCharacterController>())
        {
            if (character != null)
            {
                Destroy(character.gameObject);
            }
        }

        //cleanupNetworkObjects(NetworkManager.Instance.Networker);
    }

    void ResetServer()
    {

        if (NetworkManager.Instance != null && NetworkManager.Instance.Networker != null)
        {
            if (networkObject != null)
            {
                networkObject.SendRpc(RPC_ALL_PLAYERS_LEAVE_LOBBY, Receivers.All);
            }

            DeleteObjects();

            Debug.Log("about to disconnect server.");

            lock (NetworkManager.Instance.Networker.Players)
            {
                foreach(var player in NetworkManager.Instance.Networker.Players)
                {
                    if (player != null)
                    {
                        ((IServer)NetworkManager.Instance.Networker).Disconnect(player, true);
                    }
                }
                ((IServer)NetworkManager.Instance.Networker).CommitDisconnects();
            }

            NetworkManager.Instance.Disconnect();
            Debug.Log("server disconnected.");


            StartCoroutine(BootUpServerAgain());

            Debug.Log("booting up server again...");

            serverHasBeenReset = true;
            status = Server.AuthStatus.Available;
        }

    }

    private void cleanupNetworkObjects(NetWorker owner, NetworkingPlayer player = null, List<System.Type> specificBehaviors = null)
    {
        if (NetworkManager.Instance == null)
            return;

        toDelete.Clear();
        MainThreadManager.Run(() =>
        {
            foreach (var no in owner.NetworkObjectList)
            {
                if (no == null)
                    continue;

                if (player != null && no.Owner != player)
                    continue;

                //delete specific stuff
                if (specificBehaviors != null)
                {
                    INetworkBehavior behav = no.AttachedBehavior;

                    System.Type tempType = behav.GetType();
                    if (specificBehaviors.Contains(behav.GetType()))
                        toDelete.Add(no);

                }
                else
                {
                    toDelete.Add(no);
                }
            }

            if (toDelete.Count > 0)
            {
                for (int i = toDelete.Count - 1; i >= 0; i--)
                {
                    owner.NetworkObjectList.Remove(toDelete[i]);
                    toDelete[i].Destroy();
                }
            }
        });
    }


    IEnumerator BootUpServerAgain()
    {
        Debug.Log("Waiting 15 seconds to reboot...");
        yield return new WaitForSeconds(15);
        Debug.Log("15 seconds up, beginning reboot sequence...");

        Server serverObject = FindObjectOfType<Server>();
        if (serverObject != null)
        {
            Destroy(serverObject.gameObject);
        }

        Debug.Log("loaded server scene again.");

        SceneManager.LoadScene("Server");

        Debug.Log("loaded server scene again.");

        Destroy(gameObject);

    }

    void OnDisable()
    {
        Debug.Log("gamemode.cs disbaled");
        if (NetworkManager.Instance != null && NetworkManager.Instance.Networker != null)
        {
            NetworkManager.Instance.Networker.playerAccepted -= Networker_playerAccepted;
            NetworkManager.Instance.Networker.playerDisconnected -= Networker_playerDisconnected;
            NetworkManager.Instance.Networker.playerRejected -= Networker_playerRejected;
            NetworkManager.Instance.Networker.playerTimeout -= Networker_playerTimeout;
            NetworkManager.Instance.Networker.playerConnected -= Networker_playerConnected;
            NetworkManager.Instance.Networker.disconnected -= Networker_disconnected;
        }
    }

    void OnDestroy()
    {
        Debug.Log("Destroyed gamemode.cs");
    }


    void Start()
    {
        if (!networkObject.IsServer)
        {
            return;
        }

        networkObject.matchTimer = initialMatchTimer;

        Debug.Log("Game mode is being initialized!");

        NetworkManager.Instance.Networker.playerAccepted += Networker_playerAccepted;
        NetworkManager.Instance.Networker.playerDisconnected += Networker_playerDisconnected;
        NetworkManager.Instance.Networker.playerRejected += Networker_playerRejected;
        NetworkManager.Instance.Networker.playerTimeout += Networker_playerTimeout;
        NetworkManager.Instance.Networker.playerConnected += Networker_playerConnected;
        NetworkManager.Instance.Networker.disconnected += Networker_disconnected;


        //NetworkManager.Instance.Networker.SetUserAuthenticator(this);

    }

    private void Networker_disconnected(NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            Debug.Log("Networker disconnected, server should be fully offline now.");
        });
    }

    private void Networker_playerConnected(NetworkingPlayer player, NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            Debug.Log("playerConnected");
        });
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
            Debug.Log("playuer accepted");
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Spawn Point");

            Vector3 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
            Debug.Log("Found spawn points");

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
            if (NetworkManager.Instance != null && NetworkManager.Instance.Networker != null)
            {
                Debug.Log("Player Count: " + NetworkManager.Instance.Networker.Players.Count);
                //NetworkManager.Instance.UpdateMasterServerListing(NetworkManager.Instance.Networker, "Opus", "BattleRoyale", "Solo");

                Debug.Log("Disconnect");
                //Loop through all players and find the player who disconnected, store all it's networkobjects to a list
                List<NetworkObject> toDelete = new List<NetworkObject>();

                if (sender != null && sender.NetworkObjectList.Count > 0)
                {
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
                }

                //Remove the actual network object outside of the foreach loop, as we would modify the collection at runtime elsewise. (could also use a return, too late)
                if (sender != null)
                {
                    if (toDelete.Count > 0)
                    {
                        for (int i = toDelete.Count - 1; i >= 0; i--)
                        {
                            sender.NetworkObjectList.Remove(toDelete[i]);
                            toDelete[i].Destroy();
                        }
                    }
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
