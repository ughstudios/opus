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

    }

    void SetupTerrain()
    {
        tm = FindObjectOfType<TerrainManager>();
        tm.follow.Add(gameObject);
        tm.StartGeneration();
    }

    void Update()
    {
        if (tm == null)
        {
            if (!NetworkManager.Instance.IsServer && characterController.networkObject.IsOwner)
                SetupTerrain();
            return;
        }

        if (tm.TerrainExistsAt(transform.position))
        {
            characterController.enabled = true;
            capsuleCollider.enabled = true;
            unityCharacterController.enabled = true;

            Destroy(LoadingScreenUI);
            Destroy(this);

        }

    }

}
