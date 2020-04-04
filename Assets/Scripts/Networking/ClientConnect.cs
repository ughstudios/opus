using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine.SceneManagement;

public class ClientConnect : MonoBehaviour
{

    public string hostAddress = "127.0.0.1";
    public ushort port = 15937;

    public GameObject networkManagerPrefab;
    public NetworkManager mgr;


    private void Start()
    {
        // Do any firewall opening requests on the operating system
        NetWorker.PingForFirewall(port);

        Rpc.MainThreadRunner = MainThreadManager.Instance;

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
