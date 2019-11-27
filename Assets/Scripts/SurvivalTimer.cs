﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurvivalTimer : MonoBehaviour
{

    private PlayerController playerController;
    private GameObject player;

    public Slider waterBar;
    public Slider foodBar;

    public Slider healthBar;

    public void update_water_bar()
    {
        playerController.water = 100;
        waterBar.value = playerController.water;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        waterBar.value = 100;
        foodBar.value = 100;
        healthBar.value = 100;

        StartCoroutine(SurvivalTimerCoroutine());
    }

    IEnumerator SurvivalTimerCoroutine()
    {
        while (true)
        {

            yield return new WaitForSeconds(5);

            playerController.water -= 5;
            playerController.food -= 5;

            waterBar.value = playerController.water;
            foodBar.value = playerController.food;


            if (playerController.water <= 0) 
            {
                waterBar.value = 0;
                playerController.TakeDamage(playerController, 3);
            }
            if (playerController.food <= 0)
            {
                foodBar.value = 0;
                playerController.TakeDamage(playerController, 3);
            }

            healthBar.value = playerController.health;
        }
    }

}
