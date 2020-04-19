using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    private ClientConnect client;

    public TMP_InputField chatMessageToSend;
    public GameObject chatMessage_prefab;
    public GameObject chatContentArea;
    public ScrollRect chatScrollRect;
    
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

        chatMessageToSend.DeactivateInputField();
        chatMessageToSend.ReleaseSelection();

    }

    private void SteamMatchmaking_OnChatMessage(Lobby lobby, Friend member, string message)
    {
        GameObject chatMessageSpawned = Instantiate(chatMessage_prefab) as GameObject;
        chatMessageSpawned.transform.SetParent(chatContentArea.transform);
        TextMeshProUGUI txt = chatMessageSpawned.GetComponent<TextMeshProUGUI>();
        txt.text = message;

        Canvas.ForceUpdateCanvases();
        chatScrollRect.normalizedPosition = new Vector2(0, 0);

    }


}
