using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;
using TMPro;
using BeardedManStudios;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ClientConnect : MonoBehaviour, IUserAuthenticator
{

    public string hostAddress = "127.0.0.1";
    public ushort port = 15937;
    public string masterServerHost = "45.63.11.159"; // should use a hostname ideally. 
    public ushort masterServerPort = 15940;
    public uint steamDevAppID = 480; // Ours is: 954040

    public GameObject networkManagerPrefab;
    private NetworkManager mgr;

    public Button findMatchBtn;
    public Button exitGameBtn;
    public TextMeshProUGUI lobbyCountText;

    public int maxLobbyMembers = 1;
    bool isInLobby;
    Lobby ourLobby;
    public int LOBBY_CHECK_TIMER = 5;
    public bool gameFound = false;

    public List<SteamId> allowedSteamIDs;
    

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        // Do any firewall opening requests on the operating system
        NetWorker.PingForFirewall(port);

        Rpc.MainThreadRunner = MainThreadManager.Instance;

        if (mgr == null)
        {
            mgr = Instantiate(networkManagerPrefab).GetComponent<NetworkManager>();
        }

    }

    private void OnEnable()
    {
        try
        {
            SteamClient.Init(steamDevAppID);
            Debug.Log("SteamAPI_init successful.");
        }
        catch (Exception e)
        {
            Debug.LogError(e.StackTrace);
            Debug.LogError("Message: " + e.Message);
        }
    }

    private void OnDisable()
    {
        SteamClient.Shutdown();

    }

    private void Update()
    {
        SteamClient.RunCallbacks();
    }

    public async void FindMatch()
    {
        exitGameBtn.enabled = false;
        findMatchBtn.enabled = false;
        //allowedSteamIDs.Clear();

        Lobby[] list = await SteamMatchmaking.LobbyList.RequestAsync();
        SteamMatchmaking.OnLobbyGameCreated += SteamMatchmaking_OnLobbyGameCreated;
        SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;

        Debug.Log("Lobbies Count: " + list.Length);

        foreach (Lobby lobby in list)
        {
            Debug.Log("LobbyMemberCount: " + lobby.MemberCount);
            Debug.Log("LobbyMaxMembers: " + lobby.MaxMembers);
            if (lobby.MemberCount < lobby.MaxMembers)
            {
                Debug.Log("LobbyMemberCount < MaxMembers");
                if (steamDevAppID == 480)
                {
                    Debug.Log("AppID is 480, using dev code.");
                    if (lobby.GetData("lobbyName").Contains("opus"))
                    {
                        Debug.Log("Joining a lobby (480 app id).");
                        ourLobby = lobby;
                        await lobby.Join();
                    }
                }
                else
                {
                    Debug.Log("Joining a lobby, not 480. Using production code.");
                    ourLobby = lobby;
                    await lobby.Join();
                }
            }
        }

        if (!isInLobby)
        {
            Debug.Log("Couldn't find a lobby, creating our own.");
            var lobbyr = await SteamMatchmaking.CreateLobbyAsync(maxLobbyMembers);
            if (!lobbyr.HasValue)
            {
                Debug.Log("Couldn't create lobby.");
                return;
            }

            var lobby = lobbyr.Value;
            lobby.SetPublic();
            lobby.SetJoinable(true);
            lobby.SetData("lobbyName", "opus");

        }



    }

    private void SteamMatchmaking_OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log("Someone joined a lobby: " + friend.Name);
        lobbyCountText.text = "Lobby Count: " + lobby.MemberCount + "/" + lobby.MaxMembers;

        if (lobby.IsOwnedBy(SteamClient.SteamId))
        {
            LobbyCheck(lobby);
        }
    }

    private void SteamMatchmaking_OnLobbyCreated(Result result, Lobby lobby)
    {
        ourLobby = lobby;
        Debug.Log("lobby created");
    }

    private void OnApplicationQuit()
    {
        ourLobby.Leave();
    }


    private void SteamMatchmaking_OnLobbyEntered(Lobby lobby)
    {
        Debug.Log("Joined lobby.");

        foreach (Friend f in lobby.Members)
        {
            Debug.Log("Player in our lobby: " + f.Name);
            allowedSteamIDs.Add(f.Id);
        }


        ourLobby = lobby;
        isInLobby = true;

        if (lobby.IsOwnedBy(SteamClient.SteamId))
        {
            Debug.Log("we own the lobby");
            lobby.SetPublic();
            StartCoroutine(LobbyCheckCoRoutine(lobby));
        }
    }

    IEnumerator LobbyCheckCoRoutine(Lobby lobby)
    {
        while (!gameFound)
        {
            LobbyCheck(lobby);

            yield return new WaitForSeconds(LOBBY_CHECK_TIMER);
        }
    }

    void LobbyCheck(Lobby lobby)
    {
        Debug.Log("Lobby Members: " + lobby.MemberCount + " Max Lobby Members: " + lobby.MaxMembers);


        if (lobby.MemberCount != lobby.MaxMembers)
        {
            return;
        }

        Debug.Log("finding a server.");
        Debug.Log("master server ip: " + masterServerHost);
        Debug.Log("master server port: " + masterServerPort);
        mgr.MatchmakingServersFromMasterServer(masterServerHost, masterServerPort, 0, (response) =>
        {
            Debug.Log("response found.");
            if (response != null && response.serverResponse.Count > 0)
            {
                Debug.Log("Response.serverCount: " + response.serverResponse.Count);

                for (int i = 0; i < response.serverResponse.Count; i++)
                {
                    MasterServerResponse.Server server = response.serverResponse[i];
                    Debug.Log("Server player count: " + server.PlayerCount);
                    if (server.PlayerCount == 1)
                    {
                        hostAddress = server.Address;
                        Debug.Log("hostAddress: " + server.Address);
                        port = server.Port;
                        lobby.SetGameServer(hostAddress, port);

                        return;
                    }

                }
            }
            else
            {
                Debug.Log("isNUll" + response == null);
                Debug.Log("ServerCount: " + response.serverResponse.Count);
            }


        }, "OpusGame", "BattleRoyale", "Solo");
    }

    private void SteamMatchmaking_OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamid)
    {
        Debug.Log("Lobby owner has found us a server, connecting.");
        hostAddress = ip.ToString();
        this.port = port;
        gameFound = true;

        GetComponent<Canvas>().enabled = false; // Delete the canvas
        

        lobby.Leave();

        ConnectToServer();
    }



    public void ConnectToServer()
    {
        UDPClient client = new UDPClient();
        //client.SetUserAuthenticator(this);
        client.Connect(hostAddress, port);

        if (!client.IsBound)
        {
            Debug.LogError("NetWorker failed to bind");
            return;
        }

        if (mgr == null)
        {
            mgr = Instantiate(networkManagerPrefab).GetComponent<NetworkManager>();
        }


        mgr.Initialize(client, masterServerHost, masterServerPort);

        client.serverAccepted += OnAccepted;
        client.connectAttemptFailed += Client_connectAttemptFailed;
        client.disconnected += Client_disconnected;
    }

    private void Client_disconnected(NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            Debug.LogError("Disconnected");
        });
    }

    private void Client_connectAttemptFailed(NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            Debug.LogError("Connect attempt failed.");
        });
    }

    public void OnAccepted(NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            Debug.Log("Accepted");
        });
    }

    public void IssueChallenge(NetWorker networker, NetworkingPlayer player, Action<NetworkingPlayer, BMSByte> issueChallengeAction, Action<NetworkingPlayer> skipAuthAction)
    {
        issueChallengeAction(player, new BMSByte());
    }

    public void AcceptChallenge(NetWorker networker, BMSByte challenge, Action<BMSByte> authServerAction, Action rejectServerAction)
    {
        BMSByte by = new BMSByte();
        var binFormatter = new BinaryFormatter();
        var mStream = new MemoryStream();
        binFormatter.Serialize(mStream, allowedSteamIDs);

        var data = ObjectMapper.BMSByte(mStream.ToArray());

        authServerAction(data);
    }

    public void VerifyResponse(NetWorker networker, NetworkingPlayer player, BMSByte response, Action<NetworkingPlayer> authUserAction, Action<NetworkingPlayer> rejectUserAction)
    {
        var mStream = new MemoryStream();
        var binFormatter = new BinaryFormatter();

        var ids = response.GetByteArray(response.StartIndex());

        mStream.Write(ids, 0, ids.Length);
        mStream.Position = 0;

        allowedSteamIDs = binFormatter.Deserialize(mStream) as List<SteamId>;

        authUserAction(player);
        rejectUserAction(player);

    }
}
