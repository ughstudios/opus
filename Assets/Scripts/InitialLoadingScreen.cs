using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class InitialLoadingScreen : MonoBehaviour
{

    private NewCharacterController characterController;
    private CapsuleCollider capsuleCollider;
    public GameObject LoadingScreenUI;

    private TerrainManager tm;
    
    void Start()
    {
        characterController = GetComponent<NewCharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        tm = FindObjectOfType<TerrainManager>();
        tm.follow.Add(gameObject);
        tm.StartGeneration();

    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        {
            if (hit.collider.name.Contains("Terrain"))
            {
                characterController.enabled = true;
                capsuleCollider.enabled = true;

                Destroy(LoadingScreenUI);
                Destroy(this);
            }
        }
        
    }

}
