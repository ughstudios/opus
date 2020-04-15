﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine.SceneManagement;
using System;
using Steamworks;
using BeardedManStudios.SimpleJSON;
using BeardedManStudios;
using System.Linq;

public class Server : MonoBehaviour
{
    public int MaxPlayers = 32;
    public UDPServer server;
    public ushort port = 15937;
    public GameObject networkManagerPrefab;
    private NetworkManager mgr;
    string public_ip_addr;
	public string masterServerHost = "45.63.11.159";
	//public string masterServerHost = "127.0.0.1";
	public ushort masterServerPort = 15940;

    private int GetFirstAvailablePort(ushort startingAtPort, int maxNumberOfPortsToCheck)
    {
        var range = Enumerable.Range(startingAtPort, maxNumberOfPortsToCheck);
        var portsInUse =
            from p in range
            join used in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners()
            on p equals used.Port
            select p;

        var FirstFreeUDPPortInRange = range.Except(portsInUse).FirstOrDefault();

        if (FirstFreeUDPPortInRange > 0)
        {
            return FirstFreeUDPPortInRange;
        }
        else
        {
            return -1;
        }
    }

    private void OnEnable()
    {
    }


    private void OnDisable()
    {
    
    }

    private void Update()
    {

    }


    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Server")
            Host();

        DontDestroyOnLoad(gameObject);

    }


    public void Host()
    {
        public_ip_addr = new System.Net.WebClient().DownloadString("https://api.ipify.org"); // might be a better way to do this that isn't dependent on some third party website. Works fine for now tho. 


        // Do any firewall opening requests on the operating system
        NetWorker.PingForFirewall(port);

        Rpc.MainThreadRunner = MainThreadManager.Instance;

        var availablePort = (ushort)GetFirstAvailablePort(port, 100);
        port = availablePort;
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

        JSONNode masterServerData = null;
        if (!string.IsNullOrEmpty(masterServerHost))
        {
            string serverId = "OpusGame";
            string serverName = "Opus_Map";
            string type = "BattleRoyale";
            string mode = "Solo";

            masterServerData = mgr.MasterServerRegisterData(server, serverId, serverName, type, mode, public_ip_addr, false, 0);
            Debug.Log("Registered with: " + public_ip_addr + " as our  IP");
        }
        
        mgr.Initialize(server, masterServerHost, masterServerPort, masterServerData);

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
