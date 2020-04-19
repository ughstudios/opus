using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class InitialLoadingScreen : MonoBehaviour
{

    private PlayerController playerController;
    private CapsuleCollider capsuleCollider;
    public GameObject LoadingScreenUI;

    private TerrainManager tm;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        tm = FindObjectOfType<TerrainManager>();
        tm.follow.Add(gameObject);
        tm.StartGeneration();

    }

    void Update()
    {
        if(tm.TerrainExistsAt(transform.position))
        {
            playerController.enabled = true;
            capsuleCollider.enabled = true;

            Destroy(LoadingScreenUI);
            Destroy(this);
        }


    }

}
