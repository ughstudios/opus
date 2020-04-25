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
using System.Net;
using UnityEngine.SceneManagement;
using System.Linq;

public class ClientConnect : MonoBehaviour
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
    public SteamId ourLobbyId;
    public int LOBBY_CHECK_TIMER = 5;
    public bool gameFound = false;
    private bool tryingServer = false;
    private bool ownsLobby = false;
    private Lobby[] lobbyList = null;
    UDPClient client;

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

    public Lobby GetLobby()
    {
        foreach (var lobby in lobbyList)
        {
            if (lobby.Id == ourLobbyId)
            {
                return lobby;
            }
        }

        return new Lobby();
    }

    public async void FindMatch()
    {
        exitGameBtn.enabled = false;
        findMatchBtn.enabled = false;
        //allowedSteamIDs.Clear();

        lobbyList = await SteamMatchmaking.LobbyList.RequestAsync();
        SteamMatchmaking.OnLobbyGameCreated += SteamMatchmaking_OnLobbyGameCreated;
        SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;


        Debug.Log("Lobbies Count: " + lobbyList.Length);

        foreach (Lobby lobby in lobbyList)
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
                        ourLobbyId = lobby.Id;
                        await lobby.Join();
                    }
                }
                else
                {
                    Debug.Log("Joining a lobby, not 480. Using production code.");
                    ourLobbyId = lobby.Id;
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
        ourLobbyId = lobby.Id;
        Debug.Log("lobby created");
    }

    private void OnApplicationQuit()
    {
        if (lobbyList != null && lobbyList.Length > 0)
        {
            foreach (var lobby in lobbyList)
            {
                if (lobby.Id == ourLobbyId)
                {
                    lobby.Leave();
                }
            }
        }
    }

    private void SteamMatchmaking_OnLobbyEntered(Lobby lobby)
    {
        Debug.Log("Joined lobby.");
        lobbyCountText.text = "Lobby Count: " + lobby.MemberCount + "/" + lobby.MaxMembers;

        ourLobbyId = lobby.Id;
        isInLobby = true;

        if (lobby.IsOwnedBy(SteamClient.SteamId))
        {
            Debug.Log("we own the lobby");
            ownsLobby = true;
            lobby.SetPublic();
            StartCoroutine(LobbyCheckCoRoutine(lobby));
        }
    }

    IEnumerator LobbyCheckCoRoutine(Lobby lobby)
    {
        while (!gameFound)
        {
            if (!tryingServer)
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
                    if (server.PlayerCount <= 1)
                    {
                        hostAddress = server.Address;
                        Debug.Log("hostAddress: " + server.Address);
                        port = server.Port;
                        tryingServer = true;
                        //ConnectToServer();
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
        var parsedIP = IPAddress.Parse(ip.ToString()).ToString();
        Debug.Log("IP: " + parsedIP);
        hostAddress = parsedIP;
        this.port = port;
        gameFound = true;

        GetComponent<Canvas>().enabled = false; // Delete the canvas

        ourLobbyId = lobby.Id;

        //if (!ownsLobby)
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        client = new UDPClient();
        //client.SetUserAuthenticator(this);
        client.serverAccepted += OnAccepted;
        client.connectAttemptFailed += Client_connectAttemptFailed;
        client.disconnected += Client_disconnected;
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

    }

    private void Client_disconnected(NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.LogError("Disconnected");
            if (ownsLobby)
                tryingServer = false;


            foreach (var lobby in lobbyList)
            {
                if (lobby.Id == ourLobbyId)
                {
                    lobby.Leave();
                }
            }


            SceneManager.LoadScene("MainMenu"); // load main menu when disconnected


            Destroy(gameObject); // Destroy this so we don't have dupes. 
        });
    }

    private void Client_connectAttemptFailed(NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            Debug.LogError("Connect attempt failed.");
            if (ownsLobby)
                tryingServer = false;
        });
    }

    public void OnAccepted(NetWorker sender)
    {
        /*MainThreadManager.Run(() =>
        {
            Debug.Log("Accepted");

            if (ownsLobby)
            {
                ourLobby.SetGameServer(hostAddress, port);
                gameFound = true;
                tryingServer = false;
            }
        });*/
    }

    public void IssueChallenge(NetWorker networker, NetworkingPlayer player, Action<NetworkingPlayer, BMSByte> issueChallengeAction, Action<NetworkingPlayer> skipAuthAction)
    {
        throw new NotImplementedException();
    }

    public void AcceptChallenge(NetWorker networker, BMSByte challenge, Action<BMSByte> authServerAction, Action rejectServerAction)
    {
        Server.AuthStatus status = challenge.GetBasicType<Server.AuthStatus>();

        switch (status)
        {
            case Server.AuthStatus.Available:

                List<uint> memberIds = new List<uint>();
                /*foreach (Friend f in ourLobby.Members)
                {
                    memberIds.Add(f.Id.AccountId);
                }*/

                BMSByte response = ObjectMapper.BMSByte(SteamClient.SteamId.AccountId);

                BinaryFormatter binFor = new BinaryFormatter();
                MemoryStream memStream = new MemoryStream();
                binFor.Serialize(memStream, memberIds);

                //response.Append(ObjectMapper.BMSByte(memberIds.ToArray()));
                response.Append(ObjectMapper.BMSByte(memStream.ToArray()));
                authServerAction(response);

                memStream.Close();
                return;
            case Server.AuthStatus.Checking:
                authServerAction(ObjectMapper.BMSByte(SteamClient.SteamId.AccountId));
                return;
            case Server.AuthStatus.Closed:
                rejectServerAction();
                return;
        }
        /*
        BMSByte by = new BMSByte();
        var binFormatter = new BinaryFormatter();
        var mStream = new MemoryStream();
        binFormatter.Serialize(mStream, allowedSteamIDs);

        var data = ObjectMapper.BMSByte(mStream.ToArray());

        authServerAction(data);
        */
    }

    public void VerifyResponse(NetWorker networker, NetworkingPlayer player, BMSByte response, Action<NetworkingPlayer> authUserAction, Action<NetworkingPlayer> rejectUserAction)
    {
        /*
        var mStream = new MemoryStream();
        var binFormatter = new BinaryFormatter();

        var ids = response.GetByteArray(response.StartIndex());

        mStream.Write(ids, 0, ids.Length);
        mStream.Position = 0;

        allowedSteamIDs = binFormatter.Deserialize(mStream) as List<SteamId>;

        authUserAction(player);
        rejectUserAction(player);
        */
    }

}
