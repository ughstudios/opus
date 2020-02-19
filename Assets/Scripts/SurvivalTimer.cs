using System.Collections;
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

    public void reset_water_bar_to_full()
    {
        playerController.ResetWater();
        waterBar.value = playerController.networkObject.water;
    }

    public void update_all_bars_to_match_player_controller()
    {
        waterBar.value = playerController.networkObject.water;
        foodBar.value = playerController.networkObject.food;
        healthBar.value = playerController.networkObject.health;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        waterBar.value = 100;
        foodBar.value = 100;
        healthBar.value = 100;

        playerController.ResetStats();

        //Debug.Log("isServer: "+ playerController.networkObject.IsServer);
        //Debug.Log("isOwner:" + playerController.networkObject.IsOwner);

        if (playerController.networkObject.IsServer)
        {
            StartCoroutine(SurvivalTimerCoroutine());
        }
    }

    IEnumerator SurvivalTimerCoroutine()
    {
        while (true)
        {

            yield return new WaitForSeconds(5);


            playerController.ReduceWater(5);
            playerController.ReduceFood(5);
            
            //playerController.networkObject.water -= 5;
            //playerController.networkObject.food -= 5;

            if (playerController.networkObject.water <= 0) 
            {
                waterBar.value = 0;
                playerController.TakeDamage(playerController, 3);
                //playerController.networkObject.water = 0;
                playerController.SetWater(0);
            }
            if (playerController.networkObject.food <= 0)
            {
                foodBar.value = 0;
                playerController.TakeDamage(playerController, 3);
                //playerController.networkObject.food = 0;
                playerController.SetFood(0);
            }

        }
    }

    private void Update()
    {
        waterBar.value = playerController.networkObject.water;
        foodBar.value = playerController.networkObject.food;
        healthBar.value = playerController.networkObject.health;
    }

}
