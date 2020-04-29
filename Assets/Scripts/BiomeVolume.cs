using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeVolume : MonoBehaviour
{
    private TerrainManager.BiomeData biomeData;
    public NewCharacterController characterController;
    

    void Start()
    {
        biomeData = GetComponent<TerrainManager.BiomeData>();
        

        if (biomeData)
        {
            
        }
        
    }





}
