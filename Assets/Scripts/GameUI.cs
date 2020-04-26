using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public TextMeshProUGUI matchTimer;
    public TextMeshProUGUI playersLeft;
    private GameMode gameMode;
    public GameObject playersKilledScrollBoxContent;
    public ScrollRect playersKilledScrollBox;

    void Start()
    {
    }

    
    void Update()
    {
        if (gameMode == null)
        {
            gameMode = FindObjectOfType<GameMode>();
            return;
        }


        TimeSpan time = TimeSpan.FromSeconds(gameMode.networkObject.matchTimer);
        string stringTime = time.ToString(@"hh\:mm\:ss");

        matchTimer.text = stringTime;
        playersLeft.text = "Players Left: " + (gameMode.networkObject.playerCount - 1).ToString();
    }
}
