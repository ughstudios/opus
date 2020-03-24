using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class InitialLoadingScreen : MonoBehaviour
{

    private PlayerController playerController;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    public GameObject LoadingScreenUI;

    private TerrainManager tm;
    private FreeLookCam freeLookCam;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        freeLookCam = FindObjectOfType<FreeLookCam>();

        tm = FindObjectOfType<TerrainManager>();
        tm.follow.Add(gameObject);
        //tm.StartGeneration();

    }

    void Update()
    {
        if(tm.TerrainExistsAt(transform.position))
        {
            playerController.enabled = true;
            rb.useGravity = true;
            capsuleCollider.enabled = true;

            freeLookCam.SetTarget(playerController.transform);

            Destroy(LoadingScreenUI);
            Destroy(this);
        }


    }

}
