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
        playerController.water = 100;
        waterBar.value = playerController.water;
    }

    public void update_all_bars_to_match_player_controller()
    {
        waterBar.value = playerController.water;
        foodBar.value = playerController.food;
        healthBar.value = playerController.health;
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

            if (playerController.water <= 0) 
            {
                waterBar.value = 0;
                playerController.TakeDamage(playerController, 3);
                playerController.water = 0;
            }
            if (playerController.food <= 0)
            {
                foodBar.value = 0;
                playerController.TakeDamage(playerController, 3);
                playerController.food = 0;
            }

        }
    }

    private void Update()
    {
        waterBar.value = playerController.water;
        foodBar.value = playerController.food;
        healthBar.value = playerController.health;
    }

}
