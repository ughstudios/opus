using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reversegrav : MonoBehaviour
{

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0, 10, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
