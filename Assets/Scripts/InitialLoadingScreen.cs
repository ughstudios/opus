using BeardedManStudios.Forge.Networking.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class InitialLoadingScreen : MonoBehaviour
{

    private NewCharacterController characterController;
    private CapsuleCollider capsuleCollider;
    public GameObject LoadingScreenUI;
    private CharacterController unityCharacterController;

    private TerrainManager tm;

    void Start()
    {
        characterController = GetComponent<NewCharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        unityCharacterController = GetComponent<CharacterController>();
        tm = FindObjectOfType<TerrainManager>();


    }

    void Update()
    {
        

        if (tm != null && tm.TerrainExistsAt(transform.position))
        {
            characterController.enabled = true;
            capsuleCollider.enabled = true;
            unityCharacterController.enabled = true;

            Destroy(LoadingScreenUI);
            Destroy(this);

        }

    }

}
