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
    

    void Start()
    {
        currentTime = startingTime;

    }

    void Update()
    {
        currentTime -= 1 * Time.deltaTime;
        

        waterBar.value = currentTime;
        foodBar.value = currentTime;
    }
}
