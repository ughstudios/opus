using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine.SceneManagement;

public class Server : MonoBehaviour
{
    public int MaxPlayers = 32;
    private UDPServer server;
    public ushort port = 15937;
    public GameObject networkManagerPrefab;
    private NetworkManager mgr;

    private void Start()
    {
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
