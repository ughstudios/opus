using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodGrab : MonoBehaviour
{

    public float distance;

    private GameObject character;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        character = GameObject.Find("Character");
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(this.transform.position, character.transform.position);
        if (distance < 2 && Input.GetKeyDown(KeyCode.R))
        {
            print("you grabbed food");
            

            rb = GetComponent<Rigidbody>();
            
            transform.position = new Vector3(transform.position.x, 4f, transform.position.z);
            rb.isKinematic = false;
        }
    }

    
}
