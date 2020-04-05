using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using Steamworks;
using System;
using System.Net;

public class ClientConnect : MonoBehaviour
{

    public string hostAddress = "127.0.0.1";
    public ushort port = 15937;

    public GameObject networkManagerPrefab;
    private NetworkManager mgr;

    //Steamworks
    protected Callback<LobbyCreated_t> Callback_lobbyCreated;
    protected Callback<LobbyMatchList_t> Callback_lobbyList;
    protected Callback<LobbyEnter_t> Callback_lobbyEnter;
    protected Callback<LobbyDataUpdate_t> Callback_lobbyInfo;
    protected Callback<LobbyGameCreated_t> Callback_lobbyGameCreated;

    private int maxLobbyMembers = 20; // Should be the same as the max number of players on the server, for obvious reasons. 
    private ISteamMatchmakingServerListResponse m_ServerListResponse;

    private CSteamID localLobbySteamID;

    bool isInLobby = false;
    bool loopForServers = true;
    HServerListRequest serverListRequest;

    //End Steamworks


    private void Start()
    {
        // Do any firewall opening requests on the operating system
        NetWorker.PingForFirewall(port);

        Rpc.MainThreadRunner = MainThreadManager.Instance;
        Callback_lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        Callback_lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
        Callback_lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        Callback_lobbyInfo = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);
        Callback_lobbyGameCreated = Callback<LobbyGameCreated_t>.Create(OnLobbyGameCreated); // Called when the lobby owner calls ISteamMatchMaking.SetLobbyGameServer()

        m_ServerListResponse = new ISteamMatchmakingServerListResponse(OnServerResponded, OnServerFailedToRespond, OnRefreshComplete);

        if (SteamAPI.Init())
        {
            Debug.Log("Steam API init -- SUCCESS");
        }
        else
        {
            Debug.LogError("Steam API init -- FAILED (Are you logged into steam??)");
        }

    }

    private void OnRefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response)
    {
        // nothing
    }

    private void OnServerFailedToRespond(HServerListRequest hRequest, int iServer)
    {
        // nothing
    }

    private void OnServerResponded(HServerListRequest hRequest, int iServer)
    {
        gameserveritem_t serverItem = SteamMatchmakingServers.GetServerDetails(hRequest, iServer);
        if (serverItem.m_nMaxPlayers == 0)
        {
            string gameTags = serverItem.GetGameTags();
            IPAddress ipAddr = IPAddress.Parse(gameTags);
            uint ipAddrUint32 = BitConverter.ToUInt32(ipAddr.GetAddressBytes(), 0);

            SteamMatchmaking.SetLobbyGameServer(localLobbySteamID, ipAddrUint32, port, serverItem.m_steamID);

        }
    }

    private void OnLobbyGameCreated(LobbyGameCreated_t param)
    {
        hostAddress = param.m_unIP.ToString();
        ConnectToServer();
        SteamMatchmaking.LeaveLobby(localLobbySteamID);
        SteamMatchmakingServers.ReleaseRequest(serverListRequest);
    }

    private void OnGetLobbyInfo(LobbyDataUpdate_t param)
    {
        // Called after we call RequestLobbyData
        int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)param.m_ulSteamIDLobby);
        if (numLobbyMembers < maxLobbyMembers)
        {
            SteamMatchmaking.JoinLobby((CSteamID)param.m_ulSteamIDLobby);
        }
    }

    private void OnLobbyEntered(LobbyEnter_t param)
    {
        isInLobby = true;
        localLobbySteamID = (CSteamID)param.m_ulSteamIDLobby;
        // Do anything we want to do when joining a lobby. 
        if (SteamMatchmaking.GetLobbyOwner((CSteamID)param.m_ulSteamIDLobby) == SteamUser.GetSteamID()) // If we are the owner of the lobby
        {
            StartCoroutine(LobbyReadyCheck(param));
        }
    }

    IEnumerator LobbyReadyCheck(LobbyEnter_t param)
    {

        while (loopForServers)
        {
            int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)param.m_ulSteamIDLobby);
            if (numLobbyMembers == maxLobbyMembers)
            {
                MatchMakingKeyValuePair_t[] filters =
                {
                    new MatchMakingKeyValuePair_t { m_szKey = "appid", m_szValue = "954040" }
                };

                serverListRequest = SteamMatchmakingServers.RequestInternetServerList((AppId_t)954040, filters, (uint)filters.Length, m_ServerListResponse);

            }

            yield return new WaitForSeconds(2);
        }
    }

    private void OnGetLobbiesList(LobbyMatchList_t param)
    {
        for (int i = 0; i < param.m_nLobbiesMatching - 1; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }

        if (!isInLobby)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, maxLobbyMembers);
        }

    }

    private void OnLobbyCreated(LobbyCreated_t param)
    {
        
    }

    public void FindMatch()
    {
        SteamMatchmaking.RequestLobbyList();
    }

    public void ConnectToServer()
    {
        UDPClient client = new UDPClient();
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

        mgr.Initialize(client);

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

}
