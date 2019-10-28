using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDownTimer : MonoBehaviour
{
    float currentTime = 0f;
    float startingTime = 100f;

    public Slider waterBar;
    public Slider foodBar;

    private float waterTime = 0f;
    private float foodTime = 0f;



    

    void Start()
    {
        waterTime = startingTime;
        foodTime = startingTime;

    }

    void Update()
    {
        waterTime -= 1 * Time.deltaTime;
        foodTime -= 1 * Time.deltaTime;


        waterBar.value = waterTime;
        foodBar.value = foodTime;
    }

    public void UpdateWater(float waterValue)
    {
        if(waterTime < 94)
        {
            waterTime += waterValue;
        }
            
        print(waterTime);
    }

    public void UpdateFood(float foodValue)
    {
        if (foodTime < 94)
        {
            foodTime += foodValue;
        }

        print("Food" + foodTime);
    }
}
