using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine.SceneManagement;
using Steamworks;
using System;

public class Server : MonoBehaviour
{
    public int MaxPlayers = 32;
    public UDPServer server;
    public ushort port = 15937;
    public GameObject networkManagerPrefab;
    private NetworkManager mgr;

    // Steamworks
    public string m_strServerName = "Test Server";
    public string m_strMapName = "World";
    public int m_nMaxPlayers = 20;

    bool m_bInitialized;
    bool m_bConnectedToSteam;

    // Tells us when we have successfully connected to Steam
    protected Callback<SteamServersConnected_t> m_CallbackSteamServersConnected;

    // Tells us when there was a failure to connect to Steam
    protected Callback<SteamServerConnectFailure_t> m_CallbackSteamServersConnectFailure;

    // Tells us when we have been logged out of Steam
    protected Callback<SteamServersDisconnected_t> m_CallbackSteamServersDisconnected;

    // Tells us that Steam has set our security policy (VAC on or off)
    protected Callback<GSPolicyResponse_t> m_CallbackPolicyResponse;

    //
    // Various callback functions that Steam will call to let us know about whether we should
    // allow clients to play or we should kick/deny them.
    //
    // Tells us a client has been authenticated and approved to play by Steam (passes auth, license check, VAC status, etc...)
    protected Callback<ValidateAuthTicketResponse_t> m_CallbackGSAuthTicketResponse;

    // client connection state
    protected Callback<P2PSessionRequest_t> m_CallbackP2PSessionRequest;
    protected Callback<P2PSessionConnectFail_t> m_CallbackP2PSessionConnectFail;


    const string SERVER_VERSION = "1.0.0.0";

    // UDP port for the server to do authentication on (ie, talk to Steam on)
    const ushort AUTHENTICATION_PORT = 8766;

    // UDP port for the server to listen on
    const ushort SERVER_PORT = 27015;

    // UDP port for the master server updater to listen on
    const ushort MASTER_SERVER_UPDATER_PORT = 27016;

    public string modDir = "opus";
    public string product = "opus";
    public string gameDescription = "opus game";
    public bool isDedicated = true;

    //End Steamworks



    private void OnEnable()
    {
        m_CallbackSteamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(OnSteamServersConnected);

        m_CallbackSteamServersConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(OnSteamServersConnectFailure);
        m_CallbackSteamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(OnSteamServersDisconnected);
        m_CallbackPolicyResponse = Callback<GSPolicyResponse_t>.CreateGameServer(OnPolicyResponse);

        m_CallbackGSAuthTicketResponse = Callback<ValidateAuthTicketResponse_t>.CreateGameServer(OnValidateAuthTicketResponse);
        m_CallbackP2PSessionRequest = Callback<P2PSessionRequest_t>.CreateGameServer(OnP2PSessionRequest);
        m_CallbackP2PSessionConnectFail = Callback<P2PSessionConnectFail_t>.CreateGameServer(OnP2PSessionConnectFail);

        m_bInitialized = false;
        m_bConnectedToSteam = false;


        //EServerMode serverMode = EServerMode.eServerModeAuthenticationAndSecure; // Requires players to be "Steam Authenticated" in order to login.  (Read more here: https://partner.steamgames.com/doc/features/auth)
        EServerMode serverMode = EServerMode.eServerModeNoAuthentication; // No Auth required since steam auth hasn't been coded in.

        m_bInitialized = GameServer.Init(0, AUTHENTICATION_PORT, SERVER_PORT, MASTER_SERVER_UPDATER_PORT, serverMode, SERVER_VERSION);
        if (!m_bInitialized)
        {
            Debug.Log("SteamGameServer_Init call failed");
            return;
        }

        SteamGameServer.SetModDir(modDir);
        SteamGameServer.SetProduct(product);
        SteamGameServer.SetGameDescription(gameDescription);
        SteamGameServer.SetDedicatedServer(isDedicated);

        string public_ip_addr = new System.Net.WebClient().DownloadString("https://api.ipify.org"); // might be a better way to do this that isn't dependent on some third party website. Works fine for now tho. 
        SteamGameServer.SetGameTags(public_ip_addr);

        SteamGameServer.LogOnAnonymous();

        SteamGameServer.EnableHeartbeats(true);

    }

    private void OnDisable()
    {
        if (!m_bInitialized)
        {
            return;
        }

        SteamGameServer.EnableHeartbeats(false);
        m_CallbackSteamServersConnected.Dispose();
        SteamGameServer.LogOff();

        GameServer.Shutdown();
        m_bInitialized = false;

    }

    private void Update()
    {
        if (m_bInitialized)
        {
            return;
        }

        GameServer.RunCallbacks();

        if (m_bConnectedToSteam)
        {
            SendUpdatedServerDetailsToSteam();
        }
    }

    private void SendUpdatedServerDetailsToSteam()
    {
        SteamGameServer.SetMaxPlayerCount(m_nMaxPlayers);
        SteamGameServer.SetPasswordProtected(false);
        SteamGameServer.SetServerName(m_strServerName);
        SteamGameServer.SetMapName(m_strMapName);

    }


    private void OnP2PSessionConnectFail(P2PSessionConnectFail_t param)
    {
        Debug.Log("OnP2PSessionConnectFail Called steamIDRemote: " + param.m_steamIDRemote);
    }

    private void OnP2PSessionRequest(P2PSessionRequest_t param)
    {
        Debug.Log("OnP2PSesssionRequest Called steamIDRemote: " + param.m_steamIDRemote);

        // we'll accept a connection from anyone
        SteamGameServerNetworking.AcceptP2PSessionWithUser(param.m_steamIDRemote);
    }

    private void OnValidateAuthTicketResponse(ValidateAuthTicketResponse_t param)
    {
        Debug.Log("OnValidateAuthTicketResponse Called steamID: " + param.m_SteamID);

        if (param.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseOK)
        {

        }
    }

    private void OnPolicyResponse(GSPolicyResponse_t param)
    {
        // Check if we were able to go VAC secure or not
        if (SteamGameServer.BSecure())
        {
            Debug.Log("Server is VAC Secure!");
        }
        else
        {
            Debug.Log("Server is not VAC Secure!");
        }

        Debug.Log("Game server SteamID:" + SteamGameServer.GetSteamID().ToString());
    }

    private void OnSteamServersDisconnected(SteamServersDisconnected_t param)
    {
        m_bConnectedToSteam = false;
        Debug.Log("Server got logged out of Steam");
    }

    private void OnSteamServersConnectFailure(SteamServerConnectFailure_t param)
    {
        m_bConnectedToSteam = false;
        Debug.Log("Server failed to connect to Steam");
    }

    private void OnSteamServersConnected(SteamServersConnected_t param)
    {
        Debug.Log("Server connected to Steam successfully");
        m_bConnectedToSteam = true;

        // log on is not finished until OnPolicyResponse() is called

        // Tell Steam about our server details
        SendUpdatedServerDetailsToSteam();
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Server")
            Host();
    }


    public void Host()
    {
        // Do any firewall opening requests on the operating system
        NetWorker.PingForFirewall(port);

        Rpc.MainThreadRunner = MainThreadManager.Instance;

        server = new UDPServer(MaxPlayers);
        server.Connect("127.0.0.1", port);

        server.playerTimeout += (player, sender) =>
        {
            Debug.Log("Player " + player.NetworkId + " timed out");
        };

        if (!server.IsBound)
        {
            Debug.LogError("NetWorker failed to bind");
            return;
        }

        if (mgr == null)
        {
            mgr = Instantiate(networkManagerPrefab).GetComponent<NetworkManager>();
        }

        mgr.Initialize(server);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        server.bindSuccessful += Server_bindSuccessful;
    }

    private void Server_bindSuccessful(NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            Debug.Log("Server is running!");
        });
    }
}
