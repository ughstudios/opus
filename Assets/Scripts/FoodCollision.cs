using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodCollision : MonoBehaviour
{

    private Rigidbody rb;

    private GameObject character;

    private CountDownTimer countDown;

    public float foodValue = 5;

    void Awake()
    {
        countDown = GameObject.FindObjectOfType<CountDownTimer>();
    }

    void Start()
    {
        character = GameObject.Find("Character");
    }

    void OnCollisionEnter(Collision hit)
    {
        if (hit.transform.gameObject.name == "Floor")
        {
            rb = GetComponent<Rigidbody>();
            rb.velocity = new Vector3(0, 0, 0);
            rb.isKinematic = true;
            transform.position = new Vector3(transform.position.x, 3.2f, transform.position.z);
            
        }

        if (hit.transform.gameObject.name == "Character" && transform.position.y > 3.2f)
        {
            countDown.UpdateFood(foodValue);
            Destroy(this.gameObject);
            print(transform.position.y);
        }


    }
    
}
