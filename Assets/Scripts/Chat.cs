using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Steamworks;
using Steamworks.Data;
using UnityEngine.EventSystems;

public class Chat : MonoBehaviour
{
    ClientConnect client;
    public TMP_InputField chatMessageToSend;
    public GameObject chatMessage_prefab;
    public GameObject chatContentArea;

    void Start()
    {
        SteamMatchmaking.OnChatMessage += SteamMatchmaking_OnChatMessage;
        client = FindObjectOfType<ClientConnect>();

    }

    public void SendChatMessage()
    {
        string message = SteamClient.Name + ":" + chatMessageToSend.text;

        client.ourLobby.SendChatString(message);
        chatMessageToSend.text = "";

        chatMessageToSend.ReleaseSelection();
        chatMessageToSend.DeactivateInputField();
    }

    private void SteamMatchmaking_OnChatMessage(Lobby lobby, Friend member, string message)
    {
        GameObject chatMessageSpawned = Instantiate(chatMessage_prefab) as GameObject;
        chatMessageSpawned.transform.SetParent(chatContentArea.transform);
        TextMeshProUGUI txt = chatMessageSpawned.GetComponent<TextMeshProUGUI>();
        txt.text = message;
    }
}
