using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GameUI : MonoBehaviour
{

    public TextMeshProUGUI matchTimer;
    private GameMode gameMode;


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
        string stringTime = time.ToString(@"hh\:mm\:ss\:fff");

        matchTimer.text = stringTime;
    }
}
