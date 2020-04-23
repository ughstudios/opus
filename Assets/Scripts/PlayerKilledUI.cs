using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TMPro;

public class PlayerKilledUI : MonoBehaviour
{
    public void UpdateUI(string dyingPlayer, string killingPlayer)
    {
        GetComponentInChildren<TextMeshProUGUI>().text = dyingPlayer + " killed by " + killingPlayer;
    }
}
