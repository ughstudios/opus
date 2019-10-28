using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCollision : MonoBehaviour
{
    private CountDownTimer countDown;

    public float waterValue = 5;

    void Awake()
    {
        countDown = GameObject.FindObjectOfType<CountDownTimer> ();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnCollisionEnter(Collision hit)
    {
        if (hit.transform.gameObject.name == "Character")
        {
            countDown.UpdateWater(waterValue);
            Destroy(this.gameObject);
        }
        
    }
}
